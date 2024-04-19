namespace ApiTareas.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public bool Check { get; set; }
        public required string Task { get; set; }
    }
}
