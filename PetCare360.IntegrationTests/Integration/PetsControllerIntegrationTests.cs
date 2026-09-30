using System.Net;
using System.Net.Http.Json;
using PetCare360.Domain.Entities;
using PetCare360.IntegrationTests.FactoryFixture;

namespace PetCare360.IntegrationTests.Integration
{
    [Collection("ApiCollection")]
    public class PetsControllerIntegrationTests
    {
        private readonly HttpClient _client;

        public PetsControllerIntegrationTests(ApiFactoryFixture factory)
        {
            _client = factory.CreateClient();
        }

        //Método auxiliar: todo pet precisa de um tutor, então criamos um antes
        private async Task<int> CriarTutorAsync(string email)
        {
            var tutor = new
            {
                nmTutor = "Tutor de Teste",
                cpf = $"000.000.000-{Random.Shared.Next(10, 99)}",
                email,
                telefone = "(11) 90000-0000",
                endereco = "Rua de Teste, 1"
            };

            var response = await _client.PostAsJsonAsync("/api/Tutores", tutor);
            var criado = await response.Content.ReadFromJsonAsync<Tutor>();
            return criado!.IdTutor;
        }

        [Fact]
        public async Task CriarPet_TutorExistente_RetornaCreated()
        {
            // Arrange
            var idTutor = await CriarTutorAsync("tutor.pet.ok@petcare360.com");
            var novoPet = new
            {
                nmPet = "Amora",
                especie = "Cachorro",
                raca = "Beagle",
                peso = 12.4,
                idTutor
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Pets", novoPet);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var petCriado = await response.Content.ReadFromJsonAsync<Pet>();
            Assert.NotNull(petCriado);
            Assert.Equal("Amora", petCriado.NmPet);
            Assert.Equal(idTutor, petCriado.IdTutor);
        }

        [Fact]
        public async Task CriarPet_TutorInexistente_RetornaBadRequest()
        {
            // Arrange
            var petSemTutor = new
            {
                nmPet = "Fantasma",
                especie = "Gato",
                raca = "Siamês",
                idTutor = 999999
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Pets", petSemTutor);

            // Assert
            //a exceção sai do service, passa pelo GlobalExceptionHandler e vira ProblemDetails
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            var mensagem = await response.Content.ReadAsStringAsync();
            Assert.Contains("tutor informado não existe", mensagem);
        }

        [Fact]
        public async Task BuscarHistorico_PetComVacina_RetornaPetComRelacionamentos()
        {
            // Arrange
            var idTutor = await CriarTutorAsync("tutor.historico@petcare360.com");

            var petResponse = await _client.PostAsJsonAsync("/api/Pets", new
            {
                nmPet = "Thor",
                especie = "Cachorro",
                raca = "Pastor Alemão",
                idTutor
            });
            var pet = await petResponse.Content.ReadFromJsonAsync<Pet>();

            await _client.PostAsJsonAsync("/api/Vacinas", new
            {
                nmVacina = "Antirrábica",
                fabricante = "Zoetis",
                dtAplicacao = "2026-08-01T00:00:00",
                idPet = pet!.IdPet
            });

            // Act
            var response = await _client.GetAsync($"/api/Pets/{pet.IdPet}/historico");

            // Assert
            response.EnsureSuccessStatusCode();
            var historico = await response.Content.ReadFromJsonAsync<Pet>();
            Assert.NotNull(historico);
            Assert.Contains(historico.Vacinas, v => v.NmVacina == "Antirrábica");
        }

        [Fact]
        public async Task AtualizarPet_IdDaUrlDiferenteDoCorpo_RetornaBadRequest()
        {
            // Arrange
            var idTutor = await CriarTutorAsync("tutor.update@petcare360.com");
            var petResponse = await _client.PostAsJsonAsync("/api/Pets", new
            {
                nmPet = "Nina",
                especie = "Gato",
                raca = "Persa",
                idTutor
            });
            var pet = await petResponse.Content.ReadFromJsonAsync<Pet>();

            // Act
            var response = await _client.PutAsJsonAsync($"/api/Pets/{pet!.IdPet}", new
            {
                idPet = pet.IdPet + 500,
                nmPet = "Nina Editada",
                especie = "Gato",
                raca = "Persa",
                idTutor
            });

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeletarPet_PetInexistente_RetornaNotFound()
        {
            // Arrange
            var idInexistente = 999999;

            // Act
            var response = await _client.DeleteAsync($"/api/Pets/{idInexistente}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CicloCompleto_CriarAtualizarEDeletar_FunicionaDePontaAPonta()
        {
            // Arrange
            var idTutor = await CriarTutorAsync("tutor.ciclo@petcare360.com");

            // Act
            var criacao = await _client.PostAsJsonAsync("/api/Pets", new
            {
                nmPet = "Bidu",
                especie = "Cachorro",
                raca = "Vira-lata",
                idTutor
            });
            var pet = await criacao.Content.ReadFromJsonAsync<Pet>();

            var atualizacao = await _client.PutAsJsonAsync($"/api/Pets/{pet!.IdPet}", new
            {
                idPet = pet.IdPet,
                nmPet = "Bidu Segundo",
                especie = "Cachorro",
                raca = "Vira-lata",
                idTutor
            });

            var consulta = await _client.GetAsync($"/api/Pets/{pet.IdPet}");
            var petAtualizado = await consulta.Content.ReadFromJsonAsync<Pet>();

            var exclusao = await _client.DeleteAsync($"/api/Pets/{pet.IdPet}");
            var buscaAposExclusao = await _client.GetAsync($"/api/Pets/{pet.IdPet}");

            // Assert
            Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, atualizacao.StatusCode);
            Assert.Equal("Bidu Segundo", petAtualizado!.NmPet);
            Assert.Equal(HttpStatusCode.NoContent, exclusao.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, buscaAposExclusao.StatusCode);
        }
    }
}