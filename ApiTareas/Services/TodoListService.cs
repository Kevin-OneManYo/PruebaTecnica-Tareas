using ApiTareas.Data;
using ApiTareas.IServices;
using ApiTareas.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace ApiTareas.Services
{
    public class TodoListService : ITodoListService
    {
        private DataContext _context;
        private IMemoryCache _memoryCache;
        private MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30));
        public TodoListService(DataContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        public List<Todo>? GetTodoList()
        {
            List<Todo>? TodoList = new List<Todo>();

            if (!_memoryCache.TryGetValue("TodoList", out TodoList))
            {
                TodoList = _context.TodoList.ToList();

                _memoryCache.Set("TodoList", TodoList, cacheEntryOptions);
            }

            return TodoList;
        }

        public Todo? GetTodo(int id)
        {
            Todo? todo = null;
            if (!_memoryCache.TryGetValue("Todo-" + id, out todo))
            {
                todo = _context.TodoList.Find(id);

                _memoryCache.Set("Todo-" + id, todo, cacheEntryOptions);
            }
            return todo;
        }

        public Todo NewTodo(Todo todo)
        {
            _context.TodoList.Add(todo);
            if (0 < _context.SaveChanges())
            {
                _memoryCache.Remove("TodoList");
                _memoryCache.Set("Todo-" + todo.Id, todo, cacheEntryOptions);

                if (todo.Check == false)
                {
                    List<Todo> todoList = this.GetTodoList();

                    List<Todo>? toReOrder = todoList.FindAll(x => x.Id != todo.Id && x.Order >= todo.Order && x.Check == false);

                    if (toReOrder != null && toReOrder.Count > 0 && todo.Order <= toReOrder.MaxBy(x => x.Order).Order)
                    {
                        foreach (var item in toReOrder.OrderBy(x => x.Order))
                        {
                            _memoryCache.Remove("Todo-" + item.Id);
                            Todo? OriginalTodo = _context.TodoList.Find(item.Id);
                            OriginalTodo.Order = item.Order + 1;
                            _context.SaveChanges();
                        }
                    }                    

                }
            }
            return todo;
        }

        public List<Todo> ReorderTodoList(Reorder reorder)
        {
            int lastOrder = 0;

            Todo? todo = _context.TodoList.Find(reorder.Id);
            lastOrder = todo.Order;
            todo.Order = reorder.Order;
            _context.SaveChanges();

            _memoryCache.Remove("Todo-" + reorder.Id);
            _memoryCache.Remove("TodoList");

            Todo? lastTodo = GetTodoList().Find(x => x.Id != reorder.Id && x.Check == false && x.Order == reorder.Order);
            Todo? lastTodoEntity = _context.TodoList.Find(lastTodo.Id);
            lastTodoEntity.Order = lastOrder;
            _context.SaveChanges();

            _memoryCache.Remove("Todo-" + lastTodo.Id);

            List<Todo> todoList = this.GetTodoList();
            return todoList;
        }

        public Todo Done(int id)
        {
            Todo? todo = _context.TodoList.Find(id);
            todo.Check = true;
            _context.SaveChanges();

            _memoryCache.Remove("Todo-" + id);
            _memoryCache.Remove("TodoList");

            List<Todo> todoList = this.GetTodoList();
            var toReOrder = todoList.FindAll(x => x.Order > todo.Order && x.Check == false);
            if (toReOrder != null && toReOrder.Count > 0 )
            {
                foreach (var item in toReOrder.OrderBy(x => x.Order))
                {
                    _memoryCache.Remove("Todo-" + item.Id);
                    Todo? OriginalTodo = _context.TodoList.Find(item.Id);
                    OriginalTodo.Order = item.Order - 1;
                    _context.SaveChanges();
                }
            }
            return todo;
        }
    }

}
