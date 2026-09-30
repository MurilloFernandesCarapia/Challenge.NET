using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PetCare360.API.Handlers;
using PetCare360.API.Middleware;
using PetCare360.Domain.Exceptions;

namespace PetCare360.UnitTests.Handlers
{
    public class GlobalExceptionHandlerTests
    {
        private readonly Mock<ILogger<GlobalExceptionHandler>> _mockLogger;
        private readonly GlobalExceptionHandler _handler;

        public GlobalExceptionHandlerTests()
        {
            _mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
            _handler = new GlobalExceptionHandler(_mockLogger.Object);
        }

        //Cria um HttpContext "falso" com o corpo da resposta em memória pra conseguir ler o JSON depois
        private static DefaultHttpContext CriarHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.Path = "/api/Pets";
            httpContext.Response.Body = new MemoryStream();
            return httpContext;
        }

        private static async Task<JsonElement> LerCorpoAsync(HttpContext httpContext)
        {
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var documento = await JsonDocument.ParseAsync(httpContext.Response.Body);
            return documento.RootElement.Clone();
        }

        [Fact]
        public async Task TryHandleAsync_RegraDeNegocioException_RetornaBadRequestComMensagem()
        {
            // Arrange
            var httpContext = CriarHttpContext();
            var excecao = new RegraDeNegocioException("O tutor informado não existe.");

            // Act
            var tratado = await _handler.TryHandleAsync(httpContext, excecao, CancellationToken.None);

            // Assert
            Assert.True(tratado);
            Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);

            var corpo = await LerCorpoAsync(httpContext);
            Assert.Equal("O tutor informado não existe.", corpo.GetProperty("detail").GetString());
            Assert.Equal(400, corpo.GetProperty("status").GetInt32());
        }

        [Fact]
        public async Task TryHandleAsync_DbUpdateException_RetornaConflict()
        {
            // Arrange
            var httpContext = CriarHttpContext();
            var excecao = new DbUpdateException("ORA-02292: integrity constraint violated");

            // Act
            var tratado = await _handler.TryHandleAsync(httpContext, excecao, CancellationToken.None);

            // Assert
            Assert.True(tratado);
            Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task TryHandleAsync_ExcecaoInesperada_RetornaErroInternoSemExporDetalhes()
        {
            // Arrange
            var httpContext = CriarHttpContext();
            var excecao = new InvalidOperationException("detalhe interno que não pode vazar");

            // Act
            await _handler.TryHandleAsync(httpContext, excecao, CancellationToken.None);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);

            var corpo = await LerCorpoAsync(httpContext);
            Assert.DoesNotContain("detalhe interno", corpo.GetProperty("detail").GetString());
        }

        [Fact]
        public async Task TryHandleAsync_ComCorrelationId_DevolveIdNoHeaderENoCorpo()
        {
            // Arrange
            var httpContext = CriarHttpContext();
            httpContext.Items[CorrelationIdMiddleware.ItemKey] = "abc-123";
            var excecao = new RegraDeNegocioException("O pet informado não existe.");

            // Act
            await _handler.TryHandleAsync(httpContext, excecao, CancellationToken.None);

            // Assert
            Assert.Equal("abc-123", httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());

            var corpo = await LerCorpoAsync(httpContext);
            Assert.Equal("abc-123", corpo.GetProperty("correlationId").GetString());
        }
    }
}