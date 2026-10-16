// JavaScript da tela de Projeto.
// Depois da conexão do CRUD com o banco, este arquivo ficou responsável
// apenas pelo comportamento visual do seletor de Status.
document.addEventListener("DOMContentLoaded", function () {
    const selectStatus = document.getElementById("situacaoProjeto");
    const containerStatus = document.getElementById("containerSituacao");

    // Se a tela não possuir o seletor, não há nada para fazer.
    if (!selectStatus || !containerStatus) {
        return;
    }

    function atualizarCorStatus() {
        // Remove as classes possíveis antes de aplicar a nova.
        containerStatus.classList.remove("analise", "andamento", "finalizado");

        // O value do select é o código do Status; por isso usamos o texto
        // da option selecionada para decidir somente a cor da interface.
        const texto = selectStatus.options[selectStatus.selectedIndex].text.trim();

        if (texto === "Em andamento") {
            containerStatus.classList.add("andamento");
        }
        else if (texto === "Finalizado") {
            containerStatus.classList.add("finalizado");
        }
        else {
            containerStatus.classList.add("analise");
        }
    }

    selectStatus.addEventListener("change", atualizarCorStatus);
    atualizarCorStatus();
});
