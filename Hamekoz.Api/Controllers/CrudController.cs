using Hamekoz.Api.Models;
using Hamekoz.Api.Services;

using Microsoft.AspNetCore.Mvc;

namespace Hamekoz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class CrudController<T>(ICrudService<T> service) : ControllerBase where T : IEntity
{
    // GET: api/Ts
    [HttpGet]
    public async Task<ActionResult<List<T>>> Get(CancellationToken cancellationToken)
    {
        var list = await service.GetAllAsync(cancellationToken);

        return Ok(list);
    }

    // GET: api/Ts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<T>> GetById(int id, CancellationToken cancellationToken)
    {
        var existing = await service.ReadByIdAsync(id, cancellationToken);
        return existing != null ? existing : NotFound();
    }

    // POST: api/Ts
    [HttpPost]
    public async Task<ActionResult<T>> Post([FromBody] T T, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(T, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/Ts/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put([FromBody] T T, CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(T, cancellationToken);
        return Ok(updated);
    }

    // DELETE: api/Ts/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
