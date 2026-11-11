/* ==================== MÁSCARA DE CPF / CNPJ ==================== */

// usado em todo campo que tem o atributo data-mascara:
//   data-mascara="cpf"       -> só CPF (000.000.000-00), no máximo 11 números
//   data-mascara="cpf-cnpj"  -> até 11 números é CPF; a partir do 12º vira CNPJ (00.000.000/0000-00), no máximo 14
//
// o banco guarda o documento SÓ COM NÚMEROS (ex.: 11111111000111), então este arquivo:
//   1. ao abrir a página: formata o valor que já veio do banco (perfis e edição de funcionário)
//   2. ao digitar: formata enquanto o usuário escreve
//   3. ao enviar o formulário: tira a máscara e manda só os números
//
// o <script> deste arquivo fica no fim do <body>, então os campos já existem quando ele roda
// (por isso não precisa de window.onload, que o script.js já usa)


// recebe o texto do campo e devolve ele formatado
// limite: quantos números o campo aceita (11 pro CPF, 14 pro CNPJ)
function formatarDocumento(valor, limite) {

    // \D = qualquer coisa que NÃO é número: tira pontos, traços, barras, espaços e letras
    // o slice corta o que passar do limite
    let numeros = valor.replace(/\D/g, "").slice(0, limite);

    // até 11 números: CPF -> 000.000.000-00
    if (numeros.length <= 11) {

        numeros = numeros.replace(/(\d{3})(\d)/, "$1.$2");          // 000.0
        numeros = numeros.replace(/(\d{3})(\d)/, "$1.$2");          // 000.000.0
        numeros = numeros.replace(/(\d{3})(\d{1,2})$/, "$1-$2");    // 000.000.000-00

    }
    // de 12 a 14 números: CNPJ -> 00.000.000/0000-00
    else {

        numeros = numeros.replace(/^(\d{2})(\d)/, "$1.$2");               // 00.0
        numeros = numeros.replace(/^(\d{2})\.(\d{3})(\d)/, "$1.$2.$3");   // 00.000.0
        numeros = numeros.replace(/\.(\d{3})(\d)/, ".$1/$2");             // 00.000.000/0
        numeros = numeros.replace(/(\d{4})(\d)/, "$1-$2");                // 00.000.000/0000-00

    }

    return numeros;
}


// confere se a quantidade de números está certa e avisa o navegador
// setCustomValidity("texto") marca o campo como inválido: o navegador bloqueia o envio do formulário e mostra o texto
// setCustomValidity("") marca como válido de novo
// campo vazio fica válido aqui: quem cobra o preenchimento é o required do próprio input
function validarDocumento(campo, tipo) {

    const quantidade = campo.value.replace(/\D/g, "").length;

    if (quantidade === 0) {
        campo.setCustomValidity("");
    }
    else if (tipo === "cpf") {
        campo.setCustomValidity(quantidade === 11 ? "" : "O CPF precisa ter 11 números.");
    }
    else {
        // cpf-cnpj: só vale 11 (CPF) ou 14 (CNPJ); 12 e 13 não são nem um nem outro
        campo.setCustomValidity(quantidade === 11 || quantidade === 14 ? "" : "Digite um CPF (11 números) ou um CNPJ (14 números).");
    }
}


// todos os campos da página que têm o atributo data-mascara
const camposDocumento = document.querySelectorAll("[data-mascara]");

camposDocumento.forEach(function (campo) {

    // "cpf" aceita até 11 números; "cpf-cnpj" aceita até 14
    const limite = campo.dataset.mascara === "cpf" ? 11 : 14;

    // 1. ao abrir a página: formata o valor que veio do banco (ex.: 11111111000111 -> 11.111.111/0001-11)
    campo.value = formatarDocumento(campo.value, limite);
    validarDocumento(campo, campo.dataset.mascara);

    // 2. ao digitar (ou colar): formata de novo a cada mudança no campo e confere a quantidade de números
    campo.addEventListener("input", function () {
        campo.value = formatarDocumento(campo.value, limite);
        validarDocumento(campo, campo.dataset.mascara);
    });

    // 3. ao enviar o formulário: deixa só os números, que é como o banco guarda
    // campo.form é o formulário em que o campo está (nos perfis o campo é disabled e nem é enviado, mas não atrapalha)
    if (campo.form) {
        campo.form.addEventListener("submit", function () {
            campo.value = campo.value.replace(/\D/g, "");
        });
    }

});


/* ==================== MÁSCARA DE TELEFONE ==================== */

// usado em todo campo que tem o atributo data-telefone
// diferente do documento, o telefone É GRAVADO COM A MÁSCARA no banco (ex.: (17) 91025-1360),
// então aqui NÃO tem o passo de limpar ao enviar o formulário
//   11 números (celular): (17) 91025-1360
//   10 números (fixo):    (17) 3333-0006

// recebe o texto do campo e devolve ele formatado
function formatarTelefone(valor) {

    // tira tudo que não é número e corta em 11 (DDD + 9 dígitos)
    const numeros = valor.replace(/\D/g, "").slice(0, 11);

    // nada digitado: devolve vazio (o telefone é opcional)
    if (numeros.length === 0) {
        return "";
    }

    // só o DDD (ou parte dele): (1 / (17
    if (numeros.length <= 2) {
        return "(" + numeros;
    }

    const ddd = numeros.slice(0, 2);
    const resto = numeros.slice(2);

    // até 4 números depois do DDD ainda não tem traço: (17) 3333
    if (resto.length <= 4) {
        return "(" + ddd + ") " + resto;
    }

    // 8 números depois do DDD = fixo (4-4); 9 números = celular (5-4)
    // enquanto digita (5 a 8 números) o traço fica depois do 4º; quando chega o 9º, ele passa pra depois do 5º
    const corte = resto.length === 9 ? 5 : 4;

    return "(" + ddd + ") " + resto.slice(0, corte) + "-" + resto.slice(corte);
}


// confere a quantidade de números (DDD + telefone): 10 ou 11
// campo vazio é válido, porque o telefone não é obrigatório
function validarTelefone(campo) {

    const quantidade = campo.value.replace(/\D/g, "").length;

    if (quantidade === 0 || quantidade === 10 || quantidade === 11) {
        campo.setCustomValidity("");
    }
    else {
        campo.setCustomValidity("Digite o telefone com DDD: 10 números (fixo) ou 11 números (celular).");
    }
}


const camposTelefone = document.querySelectorAll("[data-telefone]");

camposTelefone.forEach(function (campo) {

    // ao abrir a página: formata o valor que veio do banco (e confere)
    campo.value = formatarTelefone(campo.value);
    validarTelefone(campo);

    // ao digitar (ou colar): formata e confere a cada mudança
    campo.addEventListener("input", function () {
        campo.value = formatarTelefone(campo.value);
        validarTelefone(campo);
    });

});
