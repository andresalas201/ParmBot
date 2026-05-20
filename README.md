# Parmbot : Un agente virtual para la lectura de noticias



## Docs

Contiene un PDF explicando los objetivos, antecedentes y metodologia de la investigación respecto a Parmbot.

## ParmBot

Contiene los archivos de Unity necesarios para correr ParmBot, para abrir ParmBot dentro de este motor simplemente se debe abrir Unity Hub, agregar un proyecto de disco y elegir este folder.

### API Key

ParmBot funciona utilizando Gemini, por lo que es necesaria una API Key que nos permita conectarnos a este LLM. La API Key debe estar en un archivo .json que se encuentra en la siguiente dirección:

    ParmBot/Assets/Resources/config.json

Y debe tener el siguiente formato:

    { "gemini_key": "<API Key>" }