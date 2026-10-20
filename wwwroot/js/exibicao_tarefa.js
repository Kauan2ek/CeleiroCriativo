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

    // pega o input escondido que guarda o código da tarefa (preenchido pelo Razor)
    const codigoTarefa =
        document.getElementById("codigoTarefa");

    // ==========================================================
    // VERIFICAR SE É UMA TAREFA NOVA OU EXISTENTE
    // ==========================================================
    // código 0 = tarefa nova (o usuário clicou em criar, o form vai pro Create)
    // código maior que 0 = tarefa existente (o form vai pro Update)
    if (!codigoTarefa || codigoTarefa.value === "0") {
        // esconde o seletor de status, faz sentido só pra tarefa existente
        // o select continua sendo enviado, e o Razor já deixou "A fazer" selecionado
        if (containerSituacao) {
            containerSituacao.style.display = "none";
        }
    }
    else {
        // tarefa existente: mostra o seletor de status normalmente
        if (containerSituacao) {
            containerSituacao.style.display = "flex";
        }
    }

    // ==========================================================
    // ATUALIZAR VISUAL DO STATUS
    // ==========================================================
    // função que troca a classe do container conforme o status
    // pra mudar a cor (rosa, azul, verde)
    // os textos comparados são os values do select ("A fazer", "Em andamento", "Finalizado"),
    // escritos igual ao banco, porque a comparação é letra por letra
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


        // mostra o botão excluir somente quando a tarefa está em A Fazer
        const botaoExcluir =
            document.querySelector(".botao-excluir-tarefa");

        if (botaoExcluir) {
            if (status === "A fazer") {
                botaoExcluir.style.display = "block";
            }
            else {
                botaoExcluir.style.display = "none";
            }
        }

        // aplica a classe certa pro css pintar
        if (status === "A fazer") {
            containerSituacao.classList.add("a-fazer");
        }
        else if (status === "Em andamento") {
            containerSituacao.classList.add("andamento");
        }
        else if (status === "Finalizado") {
            containerSituacao.classList.add("finalizada");
        }
    }

    // ==========================================================
    // RECUPERAR STATUS DA TAREFA EXISTENTE
    // ==========================================================
    // usa o status que o C# já deixou selecionado no <select>
    // (se o select não existir, considera "A fazer")
    let statusInicial = situacaoTarefa ? situacaoTarefa.value : "A fazer";


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
    // querySelector("form") pega o primeiro form da página, que é o da tarefa (formTarefa)
    // os outros forms (exclusão e chat) vêm depois no html, então não entram aqui
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
        const checkbox =
            funcionario.querySelector("input[type=checkbox]");

        const botao =
            funcionario.querySelector(".botao-selecionar-funcionario");

        if (checkbox) {
            // clicar no nome/foto: o próprio label marca o checkbox, aqui só atualiza o visual
            checkbox.addEventListener("change", function () {
                funcionario.classList.toggle("selecionado", checkbox.checked);
            });
        }

        if (checkbox && botao) {
            // clicar no + / ✓: o label não reage a botão, então o JS inverte o checkbox
            botao.addEventListener("click", function () {
                checkbox.checked = !checkbox.checked;
                // mudar o .checked pelo JS não dispara o "change", por isso o visual é atualizado aqui também
                funcionario.classList.toggle("selecionado", checkbox.checked);
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

            // não precisa enviar nada daqui: os checkboxes marcados já fazem parte do form da tarefa
            // (atributo form="formTarefa"), então a equipe nova é gravada quando clica em Salvar
            //console.log("Funcionários selecionados:", selecionados);

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
    // o chat só existe em tarefa já salva (o Razor não monta ele em tarefa nova),
    // por isso cada parte abaixo confere se o elemento existe antes de usar
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
            // pega o texto digitado e remove espaços extras
            const texto = novoComentario.value.trim();

            // não envia mensagens só com espaços
            // (o required do html já barra o campo vazio, mas aceita "   ")
            if (!texto) {
                event.preventDefault();
                return;
            }

            // não salvar nem desenhar a mensagem por JavaScript aqui:
            // o formulário segue pro ComentarioController, que grava no banco,
            // e a tela recarrega já com o comentário montado pelo C#
            // atenção: não usar event.preventDefault() fora do if acima,
            // senão o formulário nunca chega no controller e nada é gravado
            // (era o que a versão antiga fazia: desenhava "(Eu)" na tela e não salvava)
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

// a exclusão em si não é feita aqui: o botão de confirmar do modal
// envia o formulário pra action Delete do TarefaController