using Newtonsoft.Json;

namespace Others.Models
{
  public enum TaskStatus
  {
    Pending,
    Started,
    Completed,
  }
  public class TaskWrapper : IWrapper<Task>
  {
    [JsonProperty("placeId")]
    public int PlaceId { get; set; }

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("status")]
    public TaskStatus Status;

    [JsonProperty("priority")]
    public byte Priority;

    [JsonIgnore]
    public Task Task { get; private set; }

    public TaskWrapper()
    {

    }

    public void LoadFromData(Task task)
    {
      Task = task;
      Name = Task.Name;
      //Priority = (byte)Math.Clamp(priority, 1, 5);
    }

    public void LoadFromSave(Task task)
    {
      Task = task;
    }
  }
}
