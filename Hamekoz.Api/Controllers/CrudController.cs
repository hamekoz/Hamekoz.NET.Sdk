using AutoMapper;

using Hamekoz.Api.Dtos;
using Hamekoz.Api.Models;
using Hamekoz.Api.Services;

using Microsoft.AspNetCore.Mvc;

namespace Hamekoz.Api.Controllers;

public abstract class CrudController<TEntity>(
    IMapper mapper,
    ICrudService<TEntity> service)
    : CrudController<TEntity, TEntity, TEntity, TEntity, TEntity>(mapper, service)
    where TEntity : Entity, IListItemDto, ICreateDto, IDetailDto, IUpdateDto
{
}

[ApiController]
[Route("api/[controller]")]
public abstract class CrudController<TEntity, TListItemDto, TCreateDto, TDetailDto, TUpdateDto>(
    IMapper mapper,
    ICrudService<TEntity> service) : ControllerBase
    where TEntity : Entity
    where TListItemDto : IListItemDto
    where TCreateDto : ICreateDto
    where TDetailDto : IDetailDto
    where TUpdateDto : IUpdateDto

{
    // GET: api/Ts
    [HttpGet]
    public async Task<ActionResult<List<TListItemDto>>> Get(CancellationToken cancellationToken)
    {
        var list = await service.GetAllAsync(cancellationToken);

        var dto = mapper.Map<List<TListItemDto>>(list);

        return Ok(dto);
    }

    // GET: api/Ts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TDetailDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await service.ReadByIdAsync(id, cancellationToken);

        var readDto = mapper.Map<TDetailDto>(entity);

        return entity != null ? readDto : NotFound();
    }

    // POST: api/Ts
    [HttpPost]
    public async Task<ActionResult<TDetailDto>> Post([FromBody] TCreateDto createDto, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<TEntity>(createDto);

        var created = await service.CreateAsync(entity, cancellationToken);

        var readDto = mapper.Map<TDetailDto>(created);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/Ts/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] TUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var entity = await service.ReadByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        mapper.Map(updateDto, entity);

        var updated = await service.UpdateAsync(entity, cancellationToken);

        var readDto = mapper.Map<TDetailDto>(updated);

        return Ok(readDto);
    }

    // DELETE: api/Ts/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
