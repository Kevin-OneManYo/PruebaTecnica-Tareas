using ApiTareas.Data;
using ApiTareas.IServices;
using ApiTareas.Models;
using Microsoft.Extensions.Caching.Memory;

namespace ApiTareas.EndPoints
{
    public static class EndPoints
    {
        public static void AddEndPoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/todoList", (ITodoListService todoListService) => {
                return todoListService.GetTodoList();
            });

            app.MapGet("/todo/{id:int}", (ITodoListService todoListService, int id) =>
                todoListService.GetTodo(id) is Todo todo ? Results.Ok(todo) : Results.NotFound("Todo not found")
            );

            app.MapPost("/todo", (ITodoListService todoListService, Todo todo) => {

                var todoList = todoListService.GetTodoList();
                Todo? nTodo = null;
                if (todoList== null || todoList.Count() == 0)
                {
                    nTodo = todoListService.NewTodo(todo);
                    return Results.Ok(nTodo);
                }
                var NotDoneListMax = todoList.FindAll(x => x.Check == false).MaxBy(x => x.Order);
                if (
                    todo.Check == false && 
                    (
                        NotDoneListMax != null && 
                        todo.Order > NotDoneListMax.Order + 1
                    )
                )
                {
                    return Results.BadRequest("Todo out of bounds");
                }

                return Results.Ok(todoListService.NewTodo(todo));
            });

            app.MapPost("/reOrder", (ITodoListService todoListService, Reorder reorder) => {

                var exists = todoListService.GetTodo(reorder.Id);
                if (exists == null)
                {
                    return Results.NotFound("Todo not found");
                }
                if (exists.Check)
                {
                    return Results.BadRequest("Todo already done");
                }
                if (exists.Order == reorder.Order)
                {
                    return Results.BadRequest("Todo already on that order");
                }

                var todoList = todoListService.GetTodoList();
                var NotDoneListMax = todoList.FindAll(x => x.Check == false).MaxBy(x => x.Order);

                if (NotDoneListMax.Order < reorder.Order)
                {
                    return Results.BadRequest("Reorder out of bounds");
                }

                todoList = todoListService.ReorderTodoList(reorder);

                return Results.Ok(todoList);
            });

            app.MapPost("/done/{id:int}", (ITodoListService todoListService, int id) => {

                var exists = todoListService.GetTodo(id);
                if (exists == null)
                {
                    return Results.NotFound("Todo not found");
                }

                if (exists.Check)
                {
                    return Results.BadRequest("Todo already done");
                }
                return Results.Ok(todoListService.Done(id));
                ;
            });
        }
    }
}
