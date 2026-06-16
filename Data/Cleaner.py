import pandas as pd
data = pd.read_csv('ParmBotQuestionnaire.csv')
data = data.drop(columns=['Marca temporal', "Código"])
data = data.dropna(axis=1)
data["Caso"] = data["Caso"].replace({
    "Caso A - ParmBot interactivo": "A",
    "Caso B - ParmBot simple": "B"
})
data.to_csv('ParmBotQuestionnaire.csv', index=False)