using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Ebceys.Infrastructure.Attributes;
using Ebceys.Infrastructure.AuthorizationTestApplication.BoundedContext;
using Ebceys.Infrastructure.AuthorizationTestApplication.Client;
using Ebceys.Infrastructure.Exceptions;
using Ebceys.Infrastructure.Helpers;
using Ebceys.Infrastructure.TestApplication.BoundedContext.Requests;
using Ebceys.Infrastructure.TestApplication.BoundedContext.Responses;
using Ebceys.Infrastructure.TestApplication.Commands;
using Microsoft.AspNetCore.Mvc;
using RoutesDictionary = Ebceys.Infrastructure.TestApplication.BoundedContext.RoutesDictionary;

namespace Ebceys.Infrastructure.TestApplication.Controllers;

using ControllerInfo = RoutesDictionary.TestControllerV1;

[ApiController]
[Route(ControllerInfo.ControllerRoute)]
[ApiVersion(ControllerInfo.ApiVersion)]
public class TestController : ControllerBase
{
    [HttpGet(ControllerInfo.Methods.GetOk)]
    public IActionResult GetOk()
    {
        return Ok();
    }

    [HttpGet(ControllerInfo.Methods.GetJson)]
    public IActionResult GetJson()
    {
        return Ok(new SomeBodyResponse("ok"));
    }

    [HttpGet(ControllerInfo.Methods.GetException)]
    public IActionResult GetException()
    {
        throw new Exception("Test");
    }

    [HttpPost(ControllerInfo.Methods.PostBody)]
    public IActionResult PostBody([Required] [FromBody] SomeBodyRequest someBody)
    {
        return Ok(new SomeBodyResponse(someBody.Message));
    }

    [HttpGet(ControllerInfo.Methods.GetQuery)]
    public IActionResult GetQuery([Required] [FromQuery] int value)
    {
        return Ok(new SomeBodyResponse(value.ToString()));
    }

    [HttpPost(ControllerInfo.Methods.PostCommand)]
    public async Task<IActionResult> PostCommand(
        [Required] [FromQuery] string name,
        [FromServices] IScopedCommandExecutor commandExecutor,
        CancellationToken token)
    {
        var result =
            await commandExecutor.ExecuteAsync<AddEntityCommandContext, AddEntityCommandResult>(
                new AddEntityCommandContext(name), token);
        var res = new CommandResultResponse([new EntityResponseDto(result.Id, result.Name)]);
        return Ok(res);
    }

    [HttpGet(ControllerInfo.Methods.GetCommand)]
    [NoResponseBodyLogging]
    public async Task<IActionResult> GetCommand(
        [FromServices] IScopedCommandExecutor commandExecutor,
        CancellationToken token)
    {
        var result =
            await commandExecutor.ExecuteAsync<GetEntitiesCommandContext, GetEntitiesCommandResult>(
                new GetEntitiesCommandContext(), token);
        var array = result.Dbos.Select(EntityResponseDto.CreateFromDbo).ToArray();
        return Ok(new CommandResultResponse(array));
    }

    [HttpDelete(ControllerInfo.Methods.DeleteCommand)]
    public async Task<IActionResult> DeleteCommand(
        [Required] [FromQuery] string name,
        [FromServices] IScopedCommandExecutor commandExecutor,
        CancellationToken token)
    {
        var result =
            await commandExecutor.ExecuteAsync<DeleteElementCommandContext, DeleteElementCommandResult>(
                new DeleteElementCommandContext(name), token);
        return !result.Success ? ApiExceptionHelper.ThrowNotFound<IActionResult>("Element not found") : Ok();
    }

    [HttpPut(ControllerInfo.Methods.PutCommand)]
    [NoRequestBodyLogging]
    public async Task<IActionResult> PutCommand(
        [Required] [FromQuery] string name,
        [Required] [FromBody] ChangeNameRequest changeNameRequest,
        [FromServices] IScopedCommandExecutor commandExecutor,
        CancellationToken token)
    {
        await commandExecutor.ExecuteAsync<UpdateEntityCommandContext, UpdateEntityCommandResult>(
            new UpdateEntityCommandContext(name, changeNameRequest.NewName), token);
        return Ok();
    }

    [HttpGet(ControllerInfo.Methods.GetToken)]
    public async Task<IActionResult> GetToken(
        [FromServices] IAuthAppClient client,
        CancellationToken token)
    {
        var result = await client.GenerateTokenAsync(new GenerateTokenRequest("Vitalik"), token);

        return Ok(result);
    }

    [HttpGet(ControllerInfo.Methods.ValidateToken)]
    public async Task<IActionResult> ValidateToken(
        [FromServices] IAuthAppClient client,
        CancellationToken token)
    {
        await client.ValidateTokenAsync(token);
        return Ok();
    }

    [HttpGet(ControllerInfo.Methods.ValidateAuth)]
    public async Task<IActionResult> ValidateAuth(
        [FromServices] IAuthAppClient client,
        CancellationToken token)
    {
        await client.ValidateAuthAsync(token);
        return Ok();
    }
}