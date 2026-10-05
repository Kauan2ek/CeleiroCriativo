// quando a página termina de carregar, sobe pro topo
// útil pra quando o usuário recarrega numa âncora tipo #cadastro
window.onload = function () {
    window.scrollTo(0, 0);
};


/* ==================== CARROSSEL ==================== */

// lista com as imagens dos banners que vão rodar
const banners = [
    "imagens/banner1.png",
    "imagens/banner2.png",
    "imagens/banner3.png"
];

// guarda qual banner tá sendo mostrado no momento
let bannerAtual = 0;


// troca o src da imagem pelo banner atual da lista
function mostrarBanner() {

    document.getElementById("imagemBanner").src =
        banners[bannerAtual];

}


// avança pro próximo banner
function proximoBanner() {

    bannerAtual++;

    // se passou do último, volta pro primeiro
    if (bannerAtual >= banners.length) {
        bannerAtual = 0;
    }

    mostrarBanner();
}


// volta pro banner anterior
function anteriorBanner() {

    bannerAtual--;

    // se foi antes do primeiro, vai pro último
    if (bannerAtual < 0) {
        bannerAtual = banners.length - 1;
    }

    mostrarBanner();
}


/* ==================== CADASTRO ==================== */

// chamada quando clica no botão cadastrar
function cadastrar() {

    const nome = document.getElementById("nome").value;

    // se o nome tá vazio, avisa e para
    if (nome === "") {

        alert("Preencha o nome.");

        return;
    }

    // mensagem provisória, ainda não salva em lugar nenhum
    alert("Cadastro realizado!");

}


// ==================== TEXTO DIGITANDO ====================

// frases que vão sendo digitadas e apagadas em sequência
const frases = [
    "Transformamos ideias em experiências.",
    "Sua marca merece ser lembrada.",
    "Criatividade que dá vida aos seus projetos.",
    "Ideias criativas. Resultados reais.",
    "Sua visão. Nossa criatividade.",
    "Onde criatividade encontra estratégia."
];

// controla qual frase tá rolando, qual caractere e se tá apagando
let fraseAtual = 0;
let caractereAtual = 0;
let apagando = false;

// elemento onde o texto é escrito
const texto = document.getElementById("textoDigitando");

function digitar() {

    const frase = frases[fraseAtual];

    // modo digitando: vai adicionando caractere por caractere
    if (!apagando) {

        texto.textContent = frase.substring(0, caractereAtual + 1);

        caractereAtual++;

        // quando termina de digitar a frase inteira, começa a apagar
        // depois de uma pausa pra dar tempo de ler
        if (caractereAtual === frase.length) {

            apagando = true;

            setTimeout(digitar, 2500);

            return;
        }

    } else {

        // modo apagando: vai removendo caractere por caractere
        texto.textContent = frase.substring(0, caractereAtual - 1);

        caractereAtual--;

        // quando termina de apagar, passa pra próxima frase
        if (caractereAtual === 0) {

            apagando = false;

            fraseAtual++;

            // se acabaram as frases, volta pra primeira
            if (fraseAtual === frases.length) {
                fraseAtual = 0;
            }

        }

    }

    // chama de novo: 45ms se tá apagando (mais rápido), 80ms se tá digitando
    setTimeout(digitar, apagando ? 45 : 80);
}

// dá o pontapé inicial no efeito
digitar();