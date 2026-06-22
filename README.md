# Parmbot : Un agente virtual para la lectura de noticias

## Docs

Contiene una serie de PDFs explicando los objetivos, antecedentes y metodologia de la investigación respecto a Parmbot, la versión más reciente y avanzada es el archivo Entrega3.pdf. También se incluyen un html con un pequeño ejemplo de guión para llevar a cabo el experimento y un pdf que contiene la presentación respecto al artículo.

## Graphs

Contiene los gráficos de caja de los resultados finales de la prueba piloto realizada como parte de la investigación

## Data

Contiene los datos de la prueba piloto en .csv y un notebook que realiza limpiado de los mismos y genera gráficos a partir de estos.

## ParmBot

Contiene los archivos de Unity necesarios para correr ParmBot, para abrir ParmBot dentro de este motor simplemente se debe abrir Unity Hub, agregar un proyecto de disco y elegir este folder.

### Correr

El agente puede usarse de 2 maneras:

1. Correrlo directamente desde Unity, simplemente se presiona el botón de play en la sección superior y se escribe dentro de la ventana de juego para interactuar.

2. Desde Unity, se entra a File > Build y se crea una build real del juego, esto creara una carpeta Build dentro de la carpeta ParmBot, esta carpeta contendrá un .exe que permite correr el agente en pantalla completa. 

## BaseCase

Contiene Scripts de python para correr una versión básica de ParmBot, esta no tiene capacidad de interacción ni personificación, simplemente se pide un tema y se recibe un resumen. 

### Correr

Para correr BaseCase se debe posicionar en CLI dentro de la carpeta principal de este git y utilizar el siguiente comando:

    python BaseCase/BaseCase.py

Al empezar el programa simplemente debe escribirse en consola el tema a buscar ej: "Traspaso de poderes Costa Rica", tras unos segundos se recibirá un resumen.

## API Key

ParmBot funciona utilizando Gemini, por lo que es necesaria una API Key que nos permita conectarnos a este LLM, la busqueda de noticias se logra utilizando NewsAPI.ai, esta tambien necesita el uso de una API Key. Las API Keys deben estar en un archivo .json que se encuentra en la siguiente dirección:

    ParmBot/Assets/Resources/config.json

Y debe tener el siguiente formato:

    { "gemini_key": "<API Key>" 
      "news_key: "<NEWS API Key>"}


## Demostración

La demostración del agente está disponible en un [video de youtube](https://youtu.be/TTakmmhOpbY) 

La demostración final está disponible en un [video de youtube](https://youtu.be/84LpvFvQUb4) 