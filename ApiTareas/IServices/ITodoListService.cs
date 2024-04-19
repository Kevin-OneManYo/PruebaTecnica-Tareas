using ApiTareas.Models;

namespace ApiTareas.IServices
{
    public interface ITodoListService
    {
        List<Todo>? GetTodoList();

        Todo? GetTodo(int id);

        Todo NewTodo(Todo todo);

        List<Todo> ReorderTodoList(Reorder reorder);

        Todo Done(int id);
    }
}
