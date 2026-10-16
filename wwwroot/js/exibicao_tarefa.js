// JavaScript da tela detalhada de Tarefa.
// O banco continua sendo manipulado pelo Controller/Repository;
// este arquivo cuida apenas de comportamentos visuais da página.
document.addEventListener("DOMContentLoaded", function () {
    // ==========================================================
    // COR DO STATUS
    // ==========================================================
    const selectStatus = document.getElementById("situacaoTarefa");
    const containerStatus = document.getElementById("containerSituacao");

    function atualizarCorStatus() {
        if (!selectStatus || !containerStatus) {
            return;
        }

        containerStatus.classList.remove("a-fazer", "andamento", "finalizada");

        if (selectStatus.value === "Em andamento") {
            containerStatus.classList.add("andamento");
        }
        else if (selectStatus.value === "Finalizado") {
            containerStatus.classList.add("finalizada");
        }
        else {
            containerStatus.classList.add("a-fazer");
        }
    }

    if (selectStatus) {
        selectStatus.addEventListener("change", atualizarCorStatus);
        atualizarCorStatus();
    }

    // ==========================================================
    // MODAL DE FUNCIONÁRIOS
    // ==========================================================
    const modalFuncionarios = document.getElementById("modalFuncionarios");
    const abrirFuncionarios = document.getElementById("abrirModalFuncionarios");
    const fecharFuncionarios = document.getElementById("fecharModalFuncionarios");
    const confirmarFuncionarios = document.getElementById("confirmarFuncionarios");

    if (abrirFuncionarios && modalFuncionarios) {
        abrirFuncionarios.addEventListener("click", function () {
            modalFuncionarios.style.display = "flex";
        });
    }

    function fecharModalFuncionarios() {
        if (modalFuncionarios) {
            modalFuncionarios.style.display = "none";
        }
    }

    if (fecharFuncionarios) {
        fecharFuncionarios.addEventListener("click", fecharModalFuncionarios);
    }

    if (confirmarFuncionarios) {
        confirmarFuncionarios.addEventListener("click", fecharModalFuncionarios);
    }

    if (modalFuncionarios) {
        modalFuncionarios.addEventListener("click", function (event) {
            if (event.target === modalFuncionarios) {
                fecharModalFuncionarios();
            }
        });
    }

    // ==========================================================
    // MODAL DE COMENTÁRIOS
    // ==========================================================
    // Os comentários pertencem à própria Tarefa. O modal apenas
    // apresenta a lista já carregada pelo TarefaSqlRepository.
    const modalComentarios = document.getElementById("modalComentarios");
    const abrirComentarios = document.getElementById("iconeComentarios");
    const fecharComentarios = document.getElementById("fecharChat");

    function abrirModalComentarios() {
        if (modalComentarios) {
            modalComentarios.style.display = "flex";
        }
    }

    function fecharModalComentarios() {
        if (modalComentarios) {
            modalComentarios.style.display = "none";
        }
    }

    if (abrirComentarios) {
        abrirComentarios.addEventListener("click", abrirModalComentarios);
    }

    if (fecharComentarios) {
        fecharComentarios.addEventListener("click", fecharModalComentarios);
    }

    if (modalComentarios) {
        modalComentarios.addEventListener("click", function (event) {
            if (event.target === modalComentarios) {
                fecharModalComentarios();
            }
        });
    }

    // Fecha qualquer modal aberto com a tecla ESC.
    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape") {
            fecharModalFuncionarios();
            fecharModalComentarios();
        }
    });
});
