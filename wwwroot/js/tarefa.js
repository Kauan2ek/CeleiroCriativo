// espera o html carregar antes de rodar
document.addEventListener("DOMContentLoaded", function () {

    // ==========================================================
    // ELEMENTOS DO KANBAN
    // ==========================================================

    // pega todos os cards de tarefa que já estão no html
    const cards =
        document.querySelectorAll(".card-tarefa");

    // pega todas as colunas do kanban
    const colunas =
        document.querySelectorAll(".corpo-coluna");


    // ==========================================================
    // VERIFICAR SE A TELA É DO CLIENTE
    // ==========================================================

    // função que checa se tá na tela de tarefas do cliente
    // a url tem que conter /Tarefa/ClienteTarefa
    // serve pra bloquear arrastar e esconder o compartilhar
    function ehCliente() {

        return window.location.pathname.includes(
            "/Tarefa/ClienteTarefa"
        );
    }


    // ==========================================================
    // ATUALIZAR LINK DE ABRIR A TAREFA
    // ==========================================================

    // o link de abrir muda conforme quem tá vendo o kanban
    // gestor, funcionário ou cliente têm telas diferentes
    function atualizarLinkTarefa(card) {

        // procura o link dentro do botão expandir
        const link =
            card.querySelector(
                ".botao-expandir-tarefa a"
            );

        // se não achou, sai fora
        if (!link) {
            return;
        }

        // pega o status atual do card
        const status =
            card.dataset.status;

        // pega o nome da tarefa
        const tarefa =
            card.dataset.tarefa;

        // por padrão, o destino é a tela do gestor
        let destino =
            "/Tarefa/ExTarefaGestor";


        // se tá na tela do funcionário, muda pra tela dele
        if (
            window.location.pathname.includes(
                "/Tarefa/FuncionarioTarefa"
            )
        ) {

            destino =
                "/Tarefa/ExTarefaFuncionario";

        }

        // se tá na tela do cliente, muda pra tela dele
        else if (
            window.location.pathname.includes(
                "/Tarefa/ClienteTarefa"
            )
        ) {

            destino =
                "/Tarefa/ExTarefaCliente";
        }


        // monta o link passando status e tarefa pela url
        // encodeURIComponent protege contra caracteres estranhos
        link.href =
            destino +
            "?status=" +
            encodeURIComponent(status) +
            "&tarefa=" +
            encodeURIComponent(tarefa);
    }


    // ==========================================================
    // ATUALIZAR BOTÃO COMPARTILHAR
    // ==========================================================

    // decide quais botões aparecem dentro do card
    function atualizarBotoes(card) {

        // procura a área de ações do card
        const acoes =
            card.querySelector(".acoes-card");

        // se não existir, sai fora
        if (!acoes) {
            return;
        }


        // ======================================================
        // CLIENTE
        // ======================================================

        // cliente não pode compartilhar, então a área fica vazia
        if (ehCliente()) {

            acoes.innerHTML = "";

            return;
        }


        // ======================================================
        // GESTOR E FUNCIONÁRIO
        // ======================================================

        // gestor e funcionário têm o botão compartilhar
        acoes.innerHTML = `
            <button type="button"
                    class="botao-compartilhar">
                Compartilhar
            </button>
        `;
    }


    // ==========================================================
    // MOVER CARD PARA UMA COLUNA
    // ==========================================================

    // move o card pra coluna correspondente ao status
    function moverCardParaStatus(card, status) {

        // acha a coluna com o data-status certo
        const colunaDestino =
            document.querySelector(
                '.corpo-coluna[data-status="' +
                status +
                '"]'
            );

        // se não achou, sai fora
        if (!colunaDestino) {
            return;
        }


        // joga o card dentro da nova coluna
        colunaDestino.appendChild(card);

        // atualiza o data-status do card
        card.dataset.status =
            status;


        // salva no localStorage pra persistir a escolha
        localStorage.setItem(
            "status_tarefa_" +
            card.dataset.tarefa,
            status
        );


        // reconfigura os botões do card
        atualizarBotoes(card);

        // e o link de abrir
        atualizarLinkTarefa(card);
    }


    // ==========================================================
    // RECUPERAR STATUS SALVO
    // ==========================================================

    // checa se o card tem status salvo no navegador
    // e se tiver, move ele pra coluna certa
    function recuperarStatusSalvo(card) {

        const tarefa =
            card.dataset.tarefa;

        if (!tarefa) {
            return;
        }


        // procura o status salvo
        const statusSalvo =
            localStorage.getItem(
                "status_tarefa_" +
                tarefa
            );


        // só aplica se for um status válido
        if (
            statusSalvo === "a_fazer" ||
            statusSalvo === "andamento" ||
            statusSalvo === "finalizada"
        ) {

            moverCardParaStatus(
                card,
                statusSalvo
            );
        }
    }


    // ==========================================================
    // CONFIGURAR ARRASTAR DO CARD
    // ==========================================================

    // habilita ou não o drag do card conforme o tipo de usuário
    function configurarArrastar(card) {

        // ======================================================
        // CLIENTE
        // ======================================================

        // cliente só visualiza, não arrasta
        if (ehCliente()) {

            card.setAttribute(
                "draggable",
                "false"
            );

            return;
        }


        // ======================================================
        // GESTOR E FUNCIONÁRIO
        // ======================================================

        // gestor e funcionário podem arrastar
        card.setAttribute(
            "draggable",
            "true"
        );


        // quando começa a arrastar
        card.addEventListener(
            "dragstart",
            function () {

                // marca o card com uma classe pra achar depois no drop
                card.classList.add(
                    "arrastando"
                );
            }
        );


        // quando termina de arrastar
        card.addEventListener(
            "dragend",
            function () {

                // tira a marca
                card.classList.remove(
                    "arrastando"
                );
            }
        );
    }


    // ==========================================================
    // CRIAR NOVO CARD
    // ==========================================================

    // cria um card novo no kanban com base no título e status
    function criarCardTarefa(titulo, status) {

        // acha a coluna correspondente ao status
        const colunaDestino =
            document.querySelector(
                '.corpo-coluna[data-status="' +
                status +
                '"]'
            );

        if (!colunaDestino) {
            return;
        }


        // ======================================================
        // CRIA O CARD
        // ======================================================

        // cria a div do card
        const card =
            document.createElement("div");

        // aplica a classe do css
        card.className =
            "card-tarefa";


        // ======================================================
        // DEFINIR SE PODE ARRASTAR
        // ======================================================

        // cliente não arrasta, o resto sim
        if (ehCliente()) {

            card.setAttribute(
                "draggable",
                "false"
            );

        }
        else {

            card.setAttribute(
                "draggable",
                "true"
            );
        }


        // guarda o título e o status no dataset do card
        card.dataset.tarefa =
            titulo;

        card.dataset.status =
            status;


        // ======================================================
        // ESTRUTURA DO CARD
        // ======================================================

        // monta o html interno do card
        // escapeHtml evita que o título seja interpretado como código
        card.innerHTML = `
            <div class="conteudo-card">

                <p>
                    ${escapeHtml(titulo)}
                </p>

                <button type="button"
                        class="botao-expandir-tarefa">

                    <a href="/Tarefa/ExTarefaGestor"
                       class="botao-expandir-tarefa">

                        <img src="/imagens/abrir.png"
                             alt="Abrir tarefa">

                    </a>

                </button>

            </div>

            <div class="acoes-card"></div>
        `;


        // ======================================================
        // COLOCAR O CARD NO KANBAN
        // ======================================================

        // adiciona o card na coluna
        colunaDestino.appendChild(
            card
        );


        // ======================================================
        // CONFIGURAR ARRASTAR
        // ======================================================

        configurarArrastar(
            card
        );


        // ======================================================
        // CONFIGURAR LINK DE ABRIR
        // ======================================================

        atualizarLinkTarefa(
            card
        );


        // ======================================================
        // CONFIGURAR BOTÕES
        // ======================================================

        // decide se mostra o compartilhar
        atualizarBotoes(
            card
        );
    }


    // ==========================================================
    // PROTEÇÃO DO TÍTULO
    // ==========================================================

    // evita que o título vire html e execute algo indesejado
    // o famoso escapar de xss
    function escapeHtml(texto) {

        // cria um elemento temporário
        const div =
            document.createElement("div");

        // joga o texto dentro como texto mesmo
        div.textContent =
            texto;

        // retorna o html já escapado
        return div.innerHTML;
    }


    // ==========================================================
    // CARREGAR TAREFAS CRIADAS
    // ==========================================================

    // pega a lista de tarefas criadas salvas no localStorage
    // se não tiver nada, começa com array vazio
    const tarefasCriadas =
        JSON.parse(
            localStorage.getItem(
                "tarefas_criadas"
            )
        ) || [];


    // pra cada tarefa salva, cria o card no kanban
    tarefasCriadas.forEach(
        function (tarefa) {

            // tenta pegar um status mais recente salvo
            const statusSalvo =
                localStorage.getItem(
                    "status_tarefa_" +
                    tarefa.titulo
                );


            // usa o status salvo, senão o original, senão a_fazer
            const status =
                statusSalvo ||
                tarefa.status ||
                "a_fazer";


            // cria o card
            criarCardTarefa(
                tarefa.titulo,
                status
            );
        }
    );


    // ==========================================================
    // CONFIGURAR CARDS QUE JÁ ESTÃO NO HTML
    // ==========================================================

    // os cards fixos que já vieram no html precisam ser configurados
    cards.forEach(
        function (card) {

            // descobre em qual coluna o card tá
            const coluna =
                card.closest(
                    ".corpo-coluna"
                );


            // se achou a coluna, copia o status dela pro card
            if (coluna) {

                card.dataset.status =
                    coluna.dataset.status;
            }


            // configura se pode arrastar
            configurarArrastar(
                card
            );


            // atualiza o link de abrir
            atualizarLinkTarefa(
                card
            );


            // e os botões
            atualizarBotoes(
                card
            );


            // se tiver status salvo, move o card
            recuperarStatusSalvo(
                card
            );
        }
    );


    // ==========================================================
    // PERMITIR SOLTAR O CARD NAS COLUNAS
    // ==========================================================

    // configura cada coluna pra aceitar cards soltos
    colunas.forEach(
        function (coluna) {

            // enquanto o card passa por cima da coluna
            coluna.addEventListener(
                "dragover",
                function (event) {

                    // cliente não pode arrastar
                    if (ehCliente()) {
                        return;
                    }


                    // sem isso o drop não funciona
                    event.preventDefault();
                }
            );


            // quando o card é solto na coluna
            coluna.addEventListener(
                "drop",
                function (event) {

                    // cliente não mexe
                    if (ehCliente()) {
                        return;
                    }


                    // impede o comportamento padrão
                    event.preventDefault();


                    // acha o card que tá sendo arrastado
                    const card =
                        document.querySelector(
                            ".arrastando"
                        );


                    // se não tem card, sai
                    if (!card) {
                        return;
                    }


                    // pega o status da coluna que recebeu
                    const novoStatus =
                        coluna.dataset.status;


                    // move o card pra lá
                    coluna.appendChild(
                        card
                    );


                    // atualiza o data-status
                    card.dataset.status =
                        novoStatus;


                    // salva no localStorage
                    localStorage.setItem(
                        "status_tarefa_" +
                        card.dataset.tarefa,
                        novoStatus
                    );


                    // atualiza o link de abrir
                    atualizarLinkTarefa(
                        card
                    );
                }
            );
        }
    );


    // ==========================================================
    // BOTÃO COMPARTILHAR
    // ==========================================================

    // escuta cliques no documento todo
    // se for no compartilhar, faz o bagulho
    document.addEventListener(
        "click",
        function (event) {

            // se não foi no compartilhar, ignora
            if (
                !event.target.classList.contains(
                    "botao-compartilhar"
                )
            ) {
                return;
            }


            // trava extra pro cliente, mesmo se o botão aparecer
            if (ehCliente()) {
                return;
            }


            // acha o card pai do botão
            const card =
                event.target.closest(
                    ".card-tarefa"
                );


            // se não achou o card, sai
            if (!card) {
                return;
            }


            // confirma com o usuário
            const confirmar =
                confirm(
                    "Tem certeza que deseja compartilhar esta tarefa com o cliente?"
                );


            // se confirmou, só loga no console por enquanto
            if (confirmar) {

                console.log(
                    "Tarefa compartilhada:",
                    card.dataset.tarefa
                );
            }


            // se cancelou, não faz nada
        }
    );

});