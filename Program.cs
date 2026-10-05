// cria o builder da aplicação web, que é quem monta tudo antes de rodar
var builder = WebApplication.CreateBuilder(args);

// adiciona o suporte a MVC (controllers + views)
// sem isso os controllers com ActionResult e as views não funcionam
builder.Services.AddControllersWithViews();

// constrói a aplicação com as configurações que foram passadas
var app = builder.Build();

// libera o acesso a arquivos estáticos (css, js, imagens, etc)
// tudo que tá em wwwroot fica acessível direto pela url
app.UseStaticFiles();

// ativa o sistema de rotas do asp.net core
// necessário pra mapear url -> controller/action
app.UseRouting();

// ativa a autorização
// aqui não tem autenticação configurada, então é meio que decorativo por enquanto
app.UseAuthorization();

// define a rota padrão do projeto
// se ninguém especificar nada, vai pra Home/Index
// o {id?} é opcional, então pode vir ou não na url
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// dá o start na aplicação
// a partir daqui o servidor começa a escutar as requisições
app.Run();