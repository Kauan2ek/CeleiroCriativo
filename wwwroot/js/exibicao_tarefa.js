// espera o html carregar antes de rodar o script
document.addEventListener("DOMContentLoaded", function () {

    // ==========================================================
    // STATUS DA TAREFA
    // ==========================================================

    // pega o container que envolve o select de status
    const containerSituacao =
        document.getElementById(
            "containerSituacao"
        );

    // pega o próprio select de status
    const situacaoTarefa =
        document.getElementById(
            "situacaoTarefa"
        );

    // pega o input escondido que guarda o nome da tarefa
    const nomeTarefa =
        document.getElementById(
            "nomeTarefa"
        );


    // ==========================================================
    // IDENTIFICAR A TAREFA
    // ==========================================================

    // lê os parâmetros passados pela url
    // tipo ?status=andamento&tarefa=xyz
    const parametros =
        new URLSearchParams(
            window.location.search
        );

    // pega só o status da url
    const statusUrl =
        parametros.get("status");

    // pega só o nome da tarefa da url
    const tarefaUrl =
        parametros.get("tarefa");


    // ==========================================================
    // VERIFICAR SE É UMA TAREFA NOVA OU EXISTENTE
    // ==========================================================

    // se não veio tarefa na url, é porque o usuário clicou em criar
    if (!tarefaUrl) {

        // esconde o seletor de status, faz sentido só pra tarefa existente
        if (containerSituacao) {

            containerSituacao.style.display =
                "none";
        }

        // já deixa "a_fazer" marcado como padrão
        if (situacaoTarefa) {

            situacaoTarefa.value =
                "a_fazer";
        }

    }
    else {

        // se veio tarefa na url, estamos editando uma existente
        // então mostra o seletor de status normalmente

        if (containerSituacao) {

            containerSituacao.style.display =
                "flex";
        }
    }


    // ==========================================================
    // ATUALIZAR VISUAL DO STATUS
    // ==========================================================

    // função que troca a classe do container conforme o status
    // pra mudar a cor (rosa, azul, verde)
    function atualizarStatus(status) {

        // se algum dos elementos não existir, nem tenta
        if (
            !containerSituacao ||
            !situacaoTarefa
        ) {
            return;
        }

        // tira as classes antigas antes de por a nova
        containerSituacao.classList.remove(
            "a-fazer",
            "andamento",
            "finalizada"
        );

        // se o status não for um dos válidos, força "a_fazer"
        if (
            status !== "a_fazer" &&
            status !== "andamento" &&
            status !== "finalizada"
        ) {

            status = "a_fazer";
        }

        // sincroniza o select com o status
        situacaoTarefa.value =
            status;

        // mostra o botão excluir somente quando a tarefa está em A Fazer
        const botaoExcluir =
            document.querySelector(
                ".botao-excluir-tarefa"
            );

        if (botaoExcluir) {

            if (status === "a_fazer") {

                botaoExcluir.style.display =
                    "block";

            }
            else {

                botaoExcluir.style.display =
                    "none";
            }
        }

        // aplica a classe certa pro css pintar
        if (status === "a_fazer") {

            containerSituacao.classList.add(
                "a-fazer"
            );

        }
        else if (status === "andamento") {

            containerSituacao.classList.add(
                "andamento"
            );

        }
        else if (status === "finalizada") {

            containerSituacao.classList.add(
                "finalizada"
            );
        }
    }


    // ==========================================================
    // RECUPERAR STATUS DA TAREFA EXISTENTE
    // ==========================================================

    // começa com "a_fazer" por padrão
    let statusInicial =
        "a_fazer";

    // se existe uma tarefa na url
    if (tarefaUrl) {

        // se veio status pela url, usa esse
        if (statusUrl) {

            statusInicial =
                statusUrl;
        }

        // procura no localStorage um status salvo pra essa tarefa
        const statusSalvo =
            localStorage.getItem(
                "status_tarefa_" + tarefaUrl
            );

        // se achou e é válido, ele tem prioridade
        // sobre o que veio na url
        if (
            statusSalvo === "a_fazer" ||
            statusSalvo === "andamento" ||
            statusSalvo === "finalizada"
        ) {

            statusInicial =
                statusSalvo;
        }
    }


    // aplica o status inicial assim que a página abre
    atualizarStatus(
        statusInicial
    );


    // ==========================================================
    // ALTERAÇÃO DO STATUS
    // ==========================================================

    if (situacaoTarefa) {

        // quando o usuário troca o status no select
        // atualiza a cor na hora
        situacaoTarefa.addEventListener(
            "change",
            function () {

                const novoStatus =
                    situacaoTarefa.value;

                atualizarStatus(
                    novoStatus
                );
            }
        );
    }


    // ==========================================================
    // SALVAR
    // ==========================================================

    const formulario =
        document.querySelector(
            "form"
        );

    if (formulario) {

        formulario.addEventListener(
            "submit",
            function (event) {

                // impede o form de recarregar a página
                // a gente trata tudo no js
                event.preventDefault();


                // ==================================================
                // PEGAR O TÍTULO DIGITADO
                // ==================================================

                const tituloTarefa =
                    document.getElementById(
                        "tituloTarefa"
                    );


                // se o título tá vazio, avisa e para
                if (
                    !tituloTarefa ||
                    !tituloTarefa.value.trim()
                ) {

                    alert(
                        "Digite um título para a tarefa."
                    );

                    return;
                }


                // tira espaços extras do começo e fim
                const titulo =
                    tituloTarefa.value.trim();


                // ==================================================
                // TAREFA NOVA
                // ==================================================

                if (!tarefaUrl) {

                    // pega a lista de tarefas já salvas no navegador
                    // ou começa com array vazio
                    let tarefas =
                        JSON.parse(
                            localStorage.getItem(
                                "tarefas_criadas"
                            )
                        ) || [];


                    // checa se já existe uma tarefa com o mesmo título
                    // pra evitar duplicata
                    const tarefaExiste =
                        tarefas.some(
                            function (tarefa) {

                                return tarefa.titulo
                                    .toLowerCase() ===
                                    titulo.toLowerCase();
                            }
                        );


                    // se já tem, avisa e não faz nada
                    if (tarefaExiste) {

                        alert(
                            "Já existe uma tarefa com esse título."
                        );

                        return;
                    }


                    // adiciona a nova tarefa na lista
                    tarefas.push({

                        titulo: titulo,

                        status: "a_fazer"

                    });


                    // salva a lista de volta no localStorage
                    localStorage.setItem(
                        "tarefas_criadas",
                        JSON.stringify(tarefas)
                    );


                    // salva também o status inicial dessa tarefa
                    localStorage.setItem(
                        "status_tarefa_" + titulo,
                        "a_fazer"
                    );


                    // uns logs pra debug
                    console.log(
                        "Nova tarefa criada:",
                        titulo
                    );

                    console.log(
                        "Status:",
                        "a_fazer"
                    );


                    // manda de volta pro kanban do gestor
                    window.location.href =
                        "/Tarefa/GestorTarefa";

                    return;
                }


                // ==================================================
                // TAREFA EXISTENTE
                // ==================================================

                // pega o status atual do select
                const status =
                    situacaoTarefa.value;

                // pega o nome da tarefa do input escondido
                const tarefa =
                    nomeTarefa.value;


                // salva o status atualizado no localStorage
                localStorage.setItem(
                    "status_tarefa_" + tarefa,
                    status
                );


                // logs pra debug
                console.log(
                    "Tarefa:",
                    tarefa
                );

                console.log(
                    "Status salvo:",
                    status
                );


                // volta pro kanban
                window.location.href =
                    "/Tarefa/GestorTarefa";
            }
        );
    }


    // ==========================================================
    // MODAL DE FUNCIONÁRIOS
    // ==========================================================

    // pega o modal, os botões de abrir, cancelar e confirmar
    const modal =
        document.getElementById(
            "modalFuncionarios"
        );

    const botoesAdicionar =
        document.querySelectorAll(
            ".botao-adicionar-funcionario"
        );

    const cancelar =
        document.getElementById(
            "cancelarFuncionarios"
        );

    const confirmar =
        document.getElementById(
            "confirmarFuncionarios"
        );


    // ==========================================================
    // ABRIR MODAL
    // ==========================================================

    // qualquer botão de adicionar funcionário abre o modal
    botoesAdicionar.forEach(
        function (botao) {

            botao.addEventListener(
                "click",
                function () {

                    if (modal) {

                        modal.style.display =
                            "flex";
                    }
                }
            );
        }
    );


    // ==========================================================
    // SELECIONAR FUNCIONÁRIOS
    // ==========================================================

    // pega todos os itens de funcionário da lista
    const funcionarios =
        document.querySelectorAll(
            ".funcionario-item"
        );

    funcionarios.forEach(
        function (funcionario) {

            const botao =
                funcionario.querySelector(
                    ".botao-selecionar-funcionario"
                );

            if (botao) {

                // clicar no + (ou check) alterna a seleção
                botao.addEventListener(
                    "click",
                    function () {

                        funcionario.classList.toggle(
                            "selecionado"
                        );
                    }
                );
            }
        }
    );


    // ==========================================================
    // BOTÃO CANCELAR
    // ==========================================================

    if (cancelar) {

        // clicar em cancelar só fecha o modal
        cancelar.addEventListener(
            "click",
            function () {

                if (modal) {

                    modal.style.display =
                        "none";
                }
            }
        );
    }


    // ==========================================================
    // BOTÃO CONFIRMAR
    // ==========================================================

    if (confirmar) {

        confirmar.addEventListener(
            "click",
            function () {

                // pega só os que estão marcados como selecionados
                const selecionados =
                    document.querySelectorAll(
                        ".funcionario-item.selecionado"
                    );

                // loga no console (por enquanto não faz mais nada)
                console.log(
                    "Funcionários selecionados:",
                    selecionados
                );

                // fecha o modal
                if (modal) {

                    modal.style.display =
                        "none";
                }
            }
        );
    }


    // ==========================================================
    // BOTÃO FECHAR
    // ==========================================================

    const fechar =
        document.getElementById(
            "fecharModalFuncionarios"
        );

    if (fechar) {

        // o X no canto do modal também fecha
        fechar.addEventListener(
            "click",
            function () {

                if (modal) {

                    modal.style.display =
                        "none";
                }
            }
        );
    }

});


// ==========================================================
// EXCLUSÃO DE TAREFA
// ==========================================================

// abre o modal de confirmação de exclusão
function confirmarExclusaoTarefa() {

    document.getElementById("modalExclusaoTarefa").style.display = "flex";
}

// fecha o modal de confirmação de exclusão
function fecharModalExclusao() {

    document.getElementById("modalExclusaoTarefa").style.display = "none";
}

// exclui a tarefa de verdade
function excluirTarefa() {

    // pega o nome da tarefa atual pelo input escondido
    const tarefa = document.getElementById("nomeTarefa").value;

    // pega a lista de tarefas criadas do localStorage
    let tarefasCriadas =
        JSON.parse(localStorage.getItem("tarefas_criadas")) || [];

    // remove da lista só a tarefa com o título que a gente quer
    tarefasCriadas = tarefasCriadas.filter(function (item) {

        return item.titulo !== tarefa;

    });

    // salva a lista sem a tarefa removida
    localStorage.setItem(
        "tarefas_criadas",
        JSON.stringify(tarefasCriadas)
    );

    // também apaga o status salvo dessa tarefa, já que não existe mais
    localStorage.removeItem(
        "status_tarefa_" + tarefa
    );

    // volta pro kanban
    window.location.href = "/Tarefa/GestorTarefa";

}