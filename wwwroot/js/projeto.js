// espera o html carregar antes de rodar
document.addEventListener("DOMContentLoaded", function () {

    // pega os parâmetros da url
    const parametros =
        new URLSearchParams(window.location.search);

    const modo =
        parametros.get("modo");


    // =====================================================
    // MODO CRIAR PROJETO
    // =====================================================

    // verifica se estamos criando um projeto novo
    if (modo === "criar") {

        // pega o formulário
        const formProjeto =
            document.getElementById("formProjeto");

        // pega os elementos da tela
        const situacao =
            document.querySelector(".situacao");

        const botoesAprovacao =
            document.getElementById("botoesAprovacao");

        // esconde a situação durante a criação
        if (situacao) {

            situacao.style.display =
                "none";
        }

        // esconde os botões de aprovação
        if (botoesAprovacao) {

            botoesAprovacao.style.display =
                "none";
        }

        // verifica se o formulário existe
        if (formProjeto) {

            // quando clicar em Salvar
            formProjeto.addEventListener(
                "submit",
                function (event) {

                    // impede o formulário de recarregar a página
                    event.preventDefault();


                    // =================================================
                    // PEGAR OS CAMPOS
                    // =================================================

                    const tituloInput =
                        document.getElementById(
                            "tituloProjeto"
                        );

                    const descricaoInput =
                        document.getElementById(
                            "descricaoProjeto"
                        );

                    const comentariosInput =
                        document.getElementById(
                            "comentariosProjeto"
                        );

                    const dataInicioInput =
                        document.getElementById(
                            "dataInicio"
                        );

                    const previsaoInput =
                        document.getElementById(
                            "previsaoTermino"
                        );

                    const dataEntregaInput =
                        document.getElementById(
                            "dataEntrega"
                        );

                    const tarefaInput =
                        document.getElementById(
                            "tituloTarefa"
                        );


                    // pega o título
                    const titulo =
                        tituloInput.value.trim();


                    // verifica se o título foi preenchido
                    if (!titulo) {

                        alert(
                            "Digite o título do projeto."
                        );

                        return;
                    }


                    // =================================================
                    // RECUPERAR PROJETOS
                    // =================================================

                    // pega os projetos já salvos
                    // caso não exista nenhum, cria uma lista vazia
                    let projetos =
                        JSON.parse(
                            localStorage.getItem(
                                "projetos_criados"
                            )
                        ) || [];


                    // =================================================
                    // CRIAR NOVO PROJETO
                    // =================================================

                    const novoProjeto = {

                        // identificador único
                        id:
                            Date.now(),

                        // informações do projeto
                        titulo:
                            titulo,

                        descricao:
                            descricaoInput.value,

                        comentarios:
                            comentariosInput.value,

                        dataInicio:
                            dataInicioInput.value,

                        previsaoTermino:
                            previsaoInput.value,

                        dataEntrega:
                            dataEntregaInput.value,

                        tarefa:
                            tarefaInput.value,

                        // todo projeto criado começa em andamento
                        situacao:
                            "andamento"
                    };


                    // adiciona o novo projeto na lista
                    projetos.push(
                        novoProjeto
                    );


                    // =================================================
                    // SALVAR
                    // =================================================

                    localStorage.setItem(
                        "projetos_criados",
                        JSON.stringify(projetos)
                    );


                    // mensagem de confirmação
                    alert(
                        "Projeto criado com sucesso!"
                    );


                    // volta para a lista de projetos
                    window.location.href =
                        "/Projeto/GestorProjeto";
                }
            );
        }


        // para o código aqui quando estamos criando
        return;
    }


    // =====================================================
    // TELA GESTOR PROJETO
    // =====================================================

    // procura o container onde os projetos serão colocados
    const containerProjetos =
        document.getElementById(
            "projetosCriados"
        );


    // se encontrou o container,
    // significa que estamos na tela GestorProjeto
    if (containerProjetos) {

        // pega os projetos salvos
        const projetos =
            JSON.parse(
                localStorage.getItem(
                    "projetos_criados"
                )
            ) || [];


        // percorre todos os projetos
        projetos.forEach(function (projeto) {

            // cria uma div para representar o projeto
            const elementoProjeto =
                document.createElement("div");


            // coloca a classe que já existe no CSS
            elementoProjeto.className =
                "projeto";


            // =================================================
            // TÍTULO
            // =================================================

            const titulo =
                document.createElement("span");

            titulo.className =
                "titulo";

            titulo.textContent =
                projeto.titulo;


            // =================================================
            // CLIENTE
            // =================================================

            const cliente =
                document.createElement("span");

            cliente.className =
                "cliente";


            const imagemCliente =
                document.createElement("img");

            imagemCliente.src =
                "/imagens/Entrar.png";

            imagemCliente.alt =
                "Cliente";


            cliente.appendChild(
                imagemCliente
            );


            cliente.appendChild(
                document.createTextNode(
                    " Nome Cliente"
                )
            );


            // =================================================
            // DATA
            // =================================================

            const data =
                document.createElement("span");

            data.className =
                "data";


            // verifica se existe data de início
            if (projeto.dataInicio) {

                // transforma a data para dia/mês/ano
                const partes =
                    projeto.dataInicio.split("-");


                if (partes.length === 3) {

                    data.textContent =
                        "Data: " +
                        partes[2] +
                        "/" +
                        partes[1] +
                        "/" +
                        partes[0];

                } else {

                    data.textContent =
                        "Data: " +
                        projeto.dataInicio;
                }

            } else {

                data.textContent =
                    "Data: ??/??/??";
            }


            // =================================================
            // STATUS
            // =================================================

            const status =
                document.createElement("span");


            status.className =
                "status " +
                projeto.situacao;


            // mostra o texto do status
            if (projeto.situacao === "andamento") {

                status.textContent =
                    "Em andamento";

            } else if (
                projeto.situacao === "finalizado"
            ) {

                status.textContent =
                    "Finalizado";

            } else {

                status.textContent =
                    "Em análise";
            }


            // =================================================
            // BOTÃO ABRIR
            // =================================================

            const abrir =
                document.createElement("a");


            abrir.className =
                "abrir-projeto";


            // leva para a tela de exibição
            // levando também o id do projeto
            abrir.href =
                "/ExibicaoProjeto/ExProjetoGestor?id=" +
                projeto.id;


            const imagemAbrir =
                document.createElement("img");


            imagemAbrir.src =
                "/imagens/abrir.png";


            imagemAbrir.alt =
                "Abrir projeto";


            abrir.appendChild(
                imagemAbrir
            );


            // =================================================
            // MONTAR O PROJETO
            // =================================================

            elementoProjeto.appendChild(
                titulo
            );

            elementoProjeto.appendChild(
                cliente
            );

            elementoProjeto.appendChild(
                data
            );

            elementoProjeto.appendChild(
                status
            );

            elementoProjeto.appendChild(
                abrir
            );


            // coloca o projeto na tela
            containerProjetos.appendChild(
                elementoProjeto
            );
        });
    }


    // =====================================================
    // TELA DE EXIBIÇÃO DO PROJETO
    // =====================================================

    const select =
        document.getElementById(
            "situacaoProjeto"
        );


    const situacao =
        document.querySelector(
            ".situacao"
        );


    const botoesAprovacao =
        document.getElementById(
            "botoesAprovacao"
        );


    // =====================================================
    // CARREGAR STATUS SALVO
    // =====================================================

    // pega o id do projeto pela url
    const idProjeto =
        parametros.get("id");


    // verifica se estamos abrindo um projeto existente
    if (idProjeto && select) {

        // pega os projetos salvos
        const projetos =
            JSON.parse(
                localStorage.getItem(
                    "projetos_criados"
                )
            ) || [];


        // procura o projeto pelo id
        const projetoAtual =
            projetos.find(
                function (item) {

                    return String(item.id) ===
                        String(idProjeto);
                }
            );


        // se encontrou o projeto
        if (projetoAtual) {

            // coloca no select o status salvo
            select.value =
                projetoAtual.situacao;


            // atualiza a aparência do status
            if (situacao) {

                situacao.classList.remove(
                    "analise",
                    "andamento",
                    "finalizado"
                );


                situacao.classList.add(
                    projetoAtual.situacao
                );
            }
        }
    }


    // =====================================================
    // ATUALIZAR BOTÕES DE APROVAÇÃO
    // =====================================================

    function atualizarBotoesAprovacao() {

        // verifica se os elementos existem
        if (!botoesAprovacao ||
            !select) {

            return;
        }


        // os botões só aparecem em "Em análise"
        if (select.value === "analise") {

            botoesAprovacao.style.display =
                "flex";

        } else {

            botoesAprovacao.style.display =
                "none";
        }
    }


    // =====================================================
    // ALTERAR SITUAÇÃO
    // =====================================================

    if (select && situacao) {

        select.addEventListener(
            "change",
            function () {

                // remove classes antigas
                situacao.classList.remove(
                    "analise",
                    "andamento",
                    "finalizado"
                );


                // adiciona a nova classe
                situacao.classList.add(
                    select.value
                );


                // atualiza os botões
                atualizarBotoesAprovacao();
            }
        );
    }


    // =====================================================
    // SALVAR ALTERAÇÃO DO STATUS
    // =====================================================

    if (select) {

        const formProjeto =
            document.getElementById(
                "formProjeto"
            );


        if (formProjeto) {

            formProjeto.addEventListener(
                "submit",
                function (event) {

                    // impede o formulário de recarregar a página
                    event.preventDefault();


                    // pega o id do projeto pela url
                    const idProjeto =
                        parametros.get("id");


                    // se não tiver id,
                    // significa que não é um projeto existente
                    if (!idProjeto) {

                        return;
                    }


                    // pega os projetos salvos
                    let projetos =
                        JSON.parse(
                            localStorage.getItem(
                                "projetos_criados"
                            )
                        ) || [];


                    // procura o projeto pelo id
                    const projeto =
                        projetos.find(
                            function (item) {

                                return String(item.id) ===
                                    String(idProjeto);
                            }
                        );


                    // se encontrou o projeto
                    if (projeto) {

                        // salva o status escolhido manualmente
                        projeto.situacao =
                            select.value;


                        // salva novamente os projetos
                        localStorage.setItem(
                            "projetos_criados",
                            JSON.stringify(projetos)
                        );


                        // atualiza os botões
                        atualizarBotoesAprovacao();


                        // mensagem de confirmação
                        alert(
                            "Projeto atualizado com sucesso!"
                        );
                    }
                }
            );
        }
    }


    // =====================================================
    // EXCLUIR PROJETO
    // =====================================================

    // pega o botão de excluir
    const btnExcluirProjeto =
        document.getElementById(
            "btnExcluirProjeto"
        );


    // verifica se o botão existe na tela
    if (btnExcluirProjeto) {

        // executa quando clicar em Excluir Projeto
        btnExcluirProjeto.addEventListener(
            "click",
            function () {

                // pega o id do projeto pela url
                const idProjeto =
                    parametros.get("id");


                // verifica se existe um projeto selecionado
                if (!idProjeto) {

                    alert(
                        "Não foi possível identificar o projeto."
                    );

                    return;
                }


                // pede confirmação antes de excluir
                const confirmar =
                    confirm(
                        "Tem certeza que deseja excluir este projeto?"
                    );


                // se cancelar, não faz nada
                if (!confirmar) {

                    return;
                }


                // recupera os projetos salvos
                let projetos =
                    JSON.parse(
                        localStorage.getItem(
                            "projetos_criados"
                        )
                    ) || [];


                // remove o projeto pelo id
                projetos =
                    projetos.filter(
                        function (projeto) {

                            return String(projeto.id) !==
                                String(idProjeto);
                        }
                    );


                // salva a lista atualizada
                localStorage.setItem(
                    "projetos_criados",
                    JSON.stringify(projetos)
                );


                // informa que a exclusão foi realizada
                alert(
                    "Projeto excluído com sucesso!"
                );


                // volta para a lista de projetos
                window.location.href =
                    "/Projeto/GestorProjeto";
            }
        );
    }


    // =====================================================
    // ATUALIZAR BOTÕES AO ABRIR A TELA
    // =====================================================

    atualizarBotoesAprovacao();

});