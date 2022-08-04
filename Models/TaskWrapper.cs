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
    public long PlaceId { get; set; }

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("status")]
    public TaskStatus Status;

    [JsonProperty("priority")]
    public int Priority;

    [JsonIgnore]
    public Task Data { get; private set; }

    public TaskWrapper()
    {

    }

    public void LoadFromData(Task task)
    {
      Data = task;
      Name = Data.Name;
      //Priority = (byte)Math.Clamp(priority, 1, 5);
    }

    public void LoadFromSave(Task task)
    {
      Data = task;
    }

    public override string ToString()
    {
      return Name;
    }
  }
}
