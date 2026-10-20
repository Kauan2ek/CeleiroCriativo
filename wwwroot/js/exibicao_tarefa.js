// espera o html carregar antes de rodar o script
document.addEventListener("DOMContentLoaded", function () {
    // ==========================================================
    // STATUS DA TAREFA
    // ==========================================================
    // pega o container que envolve o select de status
    const containerSituacao =
        document.getElementById("containerSituacao");

    // pega o próprio select de status
    const situacaoTarefa =
        document.getElementById("situacaoTarefa");

    // pega o input escondido que guarda o nome da tarefa
    const nomeTarefa =
        document.getElementById("nomeTarefa");

    // ==========================================================
    // IDENTIFICAR A TAREFA
    // ==========================================================
    // lê os parâmetros passados pela url
    // tipo ?status=andamento&tarefa=xyz
    const parametros =
        new URLSearchParams(window.location.search);

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
    if (!nomeTarefa || !nomeTarefa.value) {
        // esconde o seletor de status, faz sentido só pra tarefa existente
        if (containerSituacao) {
            containerSituacao.style.display = "none";
        }

        // já deixa "a_fazer" marcado como padrão
        if (situacaoTarefa) {
            situacaoTarefa.value = "a_fazer";
        }
    }
    else {
        // se veio tarefa na url, estamos editando uma existente
        // então mostra o seletor de status normalmente
        if (containerSituacao) {
            containerSituacao.style.display = "flex";
        }
    }

    // ==========================================================
    // ATUALIZAR VISUAL DO STATUS
    // ==========================================================
    // função que troca a classe do container conforme o status
    // pra mudar a cor (rosa, azul, verde)
    function atualizarStatus(status) {
        // se algum dos elementos não existir, nem tenta
        if (!containerSituacao || !situacaoTarefa) {
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
        situacaoTarefa.value = status;

        // mostra o botão excluir somente quando a tarefa está em A Fazer
        const botaoExcluir =
            document.querySelector(".botao-excluir-tarefa");

        if (botaoExcluir) {
            if (status === "a_fazer") {
                botaoExcluir.style.display = "block";
            }
            else {
                botaoExcluir.style.display = "none";
            }
        }

        // aplica a classe certa pro css pintar
        if (status === "a_fazer") {
            containerSituacao.classList.add("a-fazer");
        }
        else if (status === "andamento") {
            containerSituacao.classList.add("andamento");
        }
        else if (status === "finalizada") {
            containerSituacao.classList.add("finalizada");
        }
    }

    // ==========================================================
    // RECUPERAR STATUS DA TAREFA EXISTENTE
    // ==========================================================
    // começa com "a_fazer" por padrão
        // usa o status que o C# já deixou selecionado no <select>
    let statusInicial = situacaoTarefa ? situacaoTarefa.value : "a_fazer";


    // aplica o status inicial assim que a página abre
    atualizarStatus(statusInicial);

    // ==========================================================
    // ALTERAÇÃO DO STATUS
    // ==========================================================
    if (situacaoTarefa) {
        // quando o usuário troca o status no select
        // atualiza a cor na hora
        situacaoTarefa.addEventListener("change", function () {
            const novoStatus = situacaoTarefa.value;
            atualizarStatus(novoStatus);
        });
    }

    // ==========================================================
    // VALIDAR FORMULÁRIO
    // ==========================================================
    const formulario =
        document.querySelector("form");

    if (formulario) {
        formulario.addEventListener("submit", function (event) {
            // confere o título antes de enviar os dados ao controller
            const tituloTarefa =
                document.getElementById("tituloTarefa");

            // se o título estiver vazio, impede o envio
            if (
                !tituloTarefa ||
                !tituloTarefa.value.trim()
            ) {
                event.preventDefault();
                alert("Digite um título para a tarefa.");
                return;
            }

            // não salvar nem alterar dados por JavaScript aqui
            // o formulário válido deve ser enviado ao controller
        });
    }

    // ==========================================================
    // MODAL DE FUNCIONÁRIOS
    // ==========================================================
    // pega o modal, os botões de abrir, cancelar e confirmar
    const modal =
        document.getElementById("modalFuncionarios");

    const botoesAdicionar =
        document.querySelectorAll(".botao-adicionar-funcionario");

    const cancelar =
        document.getElementById("cancelarFuncionarios");

    const confirmar =
        document.getElementById("confirmarFuncionarios");

    // ==========================================================
    // ABRIR MODAL
    // ==========================================================
    // qualquer botão de adicionar funcionário abre o modal
    botoesAdicionar.forEach(function (botao) {
        botao.addEventListener("click", function () {
            if (modal) {
                modal.style.display = "flex";
            }
        });
    });

    // ==========================================================
    // SELECIONAR FUNCIONÁRIOS
    // ==========================================================
    // pega todos os itens de funcionário da lista
    const funcionarios =
        document.querySelectorAll(".funcionario-item");

    funcionarios.forEach(function (funcionario) {
        const botao =
            funcionario.querySelector(".botao-selecionar-funcionario");

        if (botao) {
            // clicar no + (ou check) alterna a seleção
            botao.addEventListener("click", function () {
                funcionario.classList.toggle("selecionado");
            });
        }
    });

    // ==========================================================
    // BOTÃO CANCELAR
    // ==========================================================
    if (cancelar) {
        // clicar em cancelar só fecha o modal
        cancelar.addEventListener("click", function () {
            if (modal) {
                modal.style.display = "none";
            }
        });
    }

    // ==========================================================
    // BOTÃO CONFIRMAR
    // ==========================================================
    if (confirmar) {
        confirmar.addEventListener("click", function () {
            // pega só os que estão marcados como selecionados
            const selecionados =
                document.querySelectorAll(".funcionario-item.selecionado");

            // loga no console (por enquanto não faz mais nada)
            console.log("Funcionários selecionados:", selecionados);

            // fecha o modal
            if (modal) {
                modal.style.display = "none";
            }
        });
    }

    // ==========================================================
    // BOTÃO FECHAR
    // ==========================================================
    const fechar =
        document.getElementById("fecharModalFuncionarios");

    if (fechar) {
        // o X no canto do modal também fecha
        fechar.addEventListener("click", function () {
            if (modal) {
                modal.style.display = "none";
            }
        });
    }

    // ==========================================================
    // MODAL DE COMENTÁRIOS
    // ==========================================================
    const campoComentarios =
        document.getElementById("comentariosTarefa");

    const iconeComentarios =
        document.getElementById("iconeComentarios");

    const modalComentarios =
        document.getElementById("modalComentarios");

    const fecharChat =
        document.getElementById("fecharChat");

    const formularioChat =
        document.getElementById("formularioChat");

    const novoComentario =
        document.getElementById("novoComentario");

    const mensagensChat =
        document.getElementById("mensagensChat");

    // ==========================================================
    // ATUALIZAR RESUMO DO ÚLTIMO COMENTÁRIO
    // ==========================================================
    function atualizarResumoComentarios() {
        // verifica se os elementos necessários existem
        if (!campoComentarios || !mensagensChat) {
            return;
        }

        // pega todas as mensagens da conversa
        const mensagens =
            mensagensChat.querySelectorAll(".mensagem");

        // pega a mensagem mais recente
        const ultimaMensagem =
            mensagens[mensagens.length - 1];

        // se ainda não houver comentários
        if (!ultimaMensagem) {
            campoComentarios.value = "";
            campoComentarios.placeholder = "Nenhum comentário ainda";
            return;
        }

        // pega o nome, o texto e o horário da última mensagem
        const nome =
            ultimaMensagem.querySelector(".nome-mensagem");

        const texto =
            ultimaMensagem.querySelector("p");

        const horario =
            ultimaMensagem.querySelector(".horario-mensagem");

        // monta o resumo que aparece no campo pequeno
        campoComentarios.value =
            (nome ? nome.textContent + " " : "") +
            (texto ? texto.textContent : "") +
            (horario ? " · " + horario.textContent : "");
    }

    // ==========================================================
    // ABRIR CONVERSA
    // ==========================================================
      function abrirChat() {
        if (modalComentarios) {
            modalComentarios.style.display = "flex";

            if (mensagensChat) {
                mensagensChat.scrollTop = mensagensChat.scrollHeight;
            }

            if (novoComentario) {
                novoComentario.focus();
            }
        }
    }

    // clicar no campo pequeno abre a conversa
    if (campoComentarios) {
        campoComentarios.addEventListener("click", abrirChat);
    }

    // clicar no lápis também abre a conversa
    if (iconeComentarios) {
        iconeComentarios.addEventListener("click", abrirChat);
    }

    // ==========================================================
    // FECHAR CONVERSA
    // ==========================================================
    function fecharConversa() {
        if (modalComentarios) {
            modalComentarios.style.display = "none";
        }
    }

    // fecha pelo botão X
    if (fecharChat) {
        fecharChat.addEventListener("click", fecharConversa);
    }

    // fecha ao clicar fora da caixa da conversa
    if (modalComentarios) {
        modalComentarios.addEventListener("click", function (event) {
            if (event.target === modalComentarios) {
                fecharConversa();
            }
        });
    }

    // fecha ao apertar Esc
    document.addEventListener("keydown", function (event) {
        if (
            event.key === "Escape" &&
            modalComentarios &&
            modalComentarios.style.display === "flex"
        ) {
            fecharConversa();
        }
    });

    // ==========================================================
    // ENVIAR NOVO COMENTÁRIO
    // ==========================================================
    if (formularioChat && novoComentario && mensagensChat) {
        formularioChat.addEventListener("submit", function (event) {
            // impede a página de recarregar ao enviar a mensagem
            event.preventDefault();

            // pega o texto digitado e remove espaços extras
            const texto = novoComentario.value.trim();

            // não envia mensagens vazias
            if (!texto) {
                return;
            }

            // cria o elemento que representa a mensagem
            const mensagem = document.createElement("div");
            mensagem.classList.add("mensagem", "mensagem-eu");

            // identifica quem enviou a mensagem
            const nome = document.createElement("span");
            nome.classList.add("nome-mensagem");
            nome.textContent = "(Eu)";

            // adiciona o texto da mensagem
            const conteudo = document.createElement("p");
            conteudo.textContent = texto;

            // registra o horário atual
            const horario = document.createElement("span");
            horario.classList.add("horario-mensagem");
            horario.textContent =
                new Date().toLocaleTimeString("pt-BR", {
                    hour: "2-digit",
                    minute: "2-digit"
                });

            // organiza o conteúdo da mensagem
            mensagem.appendChild(nome);
            mensagem.appendChild(conteudo);
            mensagem.appendChild(horario);

            // adiciona a mensagem à conversa
            mensagensChat.appendChild(mensagem);

            // limpa o campo de digitação
            novoComentario.value = "";

            // atualiza o resumo da tela menor
            atualizarResumoComentarios();

            // rola a conversa até a mensagem mais recente
            mensagensChat.scrollTop = mensagensChat.scrollHeight;

            // mantém o cursor no campo de digitação
            novoComentario.focus();
        });
    }

    // atualiza o resumo ao carregar a tela
    atualizarResumoComentarios();
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

// exclusão temporariamente sem implementação local
function excluirTarefa() {
    // a exclusão deverá ser feita pelo controller usando o banco de dados
    // não remover nem simular a exclusão no navegador
    alert("A exclusão será conectada ao CRUD do banco de dados.");
    
}