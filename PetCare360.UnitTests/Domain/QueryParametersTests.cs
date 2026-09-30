using PetCare360.Domain.Pagination;

namespace PetCare360.UnitTests.Domain
{
    public class QueryParametersTests
    {
        [Fact]
        public void Construtor_SemValores_UsaValoresPadrao()
        {
            var parametros = new PetQueryParameters();

            var pagina = parametros.Pagina;
            var tamanhoPagina = parametros.TamanhoPagina;

            Assert.Equal(1, pagina);
            Assert.Equal(10, tamanhoPagina);
            Assert.True(parametros.Ascendente);
            Assert.Null(parametros.OrdenarPor);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Pagina_ValorMenorQueUm_AjustaParaPrimeiraPagina(int paginaInformada)
        {
            var parametros = new PetQueryParameters();

            parametros.Pagina = paginaInformada;

            Assert.Equal(1, parametros.Pagina);
        }

        [Fact]
        public void TamanhoPagina_AcimaDoMaximo_LimitaNoMaximoPermitido()
        {
            var parametros = new TutorQueryParameters();

            parametros.TamanhoPagina = 1000;

            Assert.Equal(QueryParameters.TamanhoMaximoPagina, parametros.TamanhoPagina);
        }

        [Fact]
        public void TamanhoPagina_ValorZero_UsaTamanhoPadrao()
        {
            var parametros = new VacinaQueryParameters();

            parametros.TamanhoPagina = 0;

            Assert.Equal(10, parametros.TamanhoPagina);
        }
    }
}