using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskRestApi.Data;

public class SomeTask
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Created;
    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    public void Start()
    {
        Status = TaskStatus.Running;
        DateTime = DateTime.UtcNow;
    }

    public void Finish()
    {
        Status = TaskStatus.Finished;
        DateTime = DateTime.UtcNow;
    }
    
}