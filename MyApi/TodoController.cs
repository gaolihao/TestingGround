
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Polly;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace MyApi;

[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase
{
    private readonly TodoContext _context;

    public TodoController(TodoContext context)
    {
        _context = context;

        if (_context.Todos.Count() == 0)
        {
            // Create a new TodoItem if collection is empty,
            // which means you can't delete all TodoItems.
            _context.Todos.Add(new Todo { Name = "Item1" });
            _context.SaveChanges();
        }
    }

    // GET: api/Todo
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Todo>>> GetTodoItems()
    {
        return await _context.Todos.ToListAsync();
    }

    // GET: api/Todo/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Todo>> GetTodoItem(long id)
    {
        var todoItem = await _context.Todos.FindAsync(id);

        if (todoItem == null)
        {
            return NotFound();
        }

        return todoItem;
    }

    // POST: api/Todo
    [HttpPost]
    public async Task<ActionResult<Todo>> PostTodoItem(Todo item)
    {
        _context.Todos.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(PostTodoItem), new { id = item.Id }, item);
    }

    // PUT: api/Todo/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTodoItem(long id, Todo item)
    {
        if (id != item.Id)
        {
            return BadRequest();
        }

        _context.Entry(item).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Todo/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodoItem(long id)
    {
        var todoItem = await _context.Todos.FindAsync(id);

        if (todoItem == null)
        {
            return NotFound();
        }

        _context.Todos.Remove(todoItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
}