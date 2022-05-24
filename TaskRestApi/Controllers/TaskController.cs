using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaskRestApi.Data;
using TaskRestApi.DbContexts;
using TaskRestApi.Services;

namespace TaskRestApi.Controllers;

[ApiController]
[Route("/task")]
public class TaskController : ControllerBase
{
    private readonly ILogger<TaskController> _logger;
    private readonly TaskService _taskService;

    public TaskController(
        ILogger<TaskController> logger,
        TaskService taskService)
    {
        _logger = logger;
        _taskService = taskService;
    }

    [HttpPost]
    public async Task<IActionResult> PostTaskAsync(CancellationToken cancellationToken)
    {
        SomeTask task = await _taskService.AddAsync(cancellationToken);
        return Accepted(task.Guid);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetTaskAsync([FromRoute] string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out Guid guid))
        {
            return BadRequest();
        }
        
        SomeTask? someTask = await _taskService.FindAsync(guid, cancellationToken);
        
        if (someTask is null)
        {
            return NotFound();
        }

        return Ok(someTask.Status.ToString());
    }
}