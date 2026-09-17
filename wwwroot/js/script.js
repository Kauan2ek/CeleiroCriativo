window.onload = function() {
    window.scrollTo(0, 0);
};
/* ==================== CARROSSEL ==================== */

/* Lista com as imagens dos banners */
const banners = [
    "imagens/banner1.png",
    "imagens/banner2.png",
    "imagens/banner3.png"
];

/* Índice do banner atual */
let bannerAtual = 0;


/* Função para mostrar o banner */
function mostrarBanner() {

    document.getElementById("imagemBanner").src =
        banners[bannerAtual];

}


/* Função para passar para o próximo banner */
function proximoBanner() {

    bannerAtual++;

    /* Volta para o primeiro banner */
    if (bannerAtual >= banners.length) {
        bannerAtual = 0;
    }

    mostrarBanner();
}


/* Função para voltar ao banner anterior */
function anteriorBanner() {

    bannerAtual--;

    /* Vai para o último banner */
    if (bannerAtual < 0) {
        bannerAtual = banners.length - 1;
    }

    mostrarBanner();
}


/* ==================== CADASTRO ==================== */

/* Função chamada ao clicar no botão */
function cadastrar() {

    const nome = document.getElementById("nome").value;

    /* Verifica se o nome foi preenchido */
    if (nome === "") {

        alert("Preencha o nome.");

        return;
    }

    /* Mensagem provisória */
    alert("Cadastro realizado!");

}

// ==================== TEXTO DIGITANDO ====================

const frases = [
    "Transformamos ideias em experiências.",
    "Sua marca merece ser lembrada.",
    "Criatividade que dá vida aos seus projetos.",
    "Ideias criativas. Resultados reais.",
    "Sua visão. Nossa criatividade.",
    "Onde criatividade encontra estratégia."
];

let fraseAtual = 0;
let caractereAtual = 0;
let apagando = false;

const texto = document.getElementById("textoDigitando");

function digitar() {

    const frase = frases[fraseAtual];

    if (!apagando) {

        texto.textContent = frase.substring(0, caractereAtual + 1);

        caractereAtual++;

        if (caractereAtual === frase.length) {

            apagando = true;

            setTimeout(digitar, 2500);

            return;
        }

    } else {

        texto.textContent = frase.substring(0, caractereAtual - 1);

        caractereAtual--;

        if (caractereAtual === 0) {

            apagando = false;

            fraseAtual++;

            if (fraseAtual === frases.length) {
                fraseAtual = 0;
            }

        }

    }

    setTimeout(digitar, apagando ? 45 : 80);
}

digitar();
