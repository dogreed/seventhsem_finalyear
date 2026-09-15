from fastapi import FastAPI
from pydantic import BaseModel
import pickle

app = FastAPI(title="TalentMap AI Recommendation Service")


# Request model
class RecommendationRequest(BaseModel):
    matchingSkills: int
    resourceSkillCount: int
    matchPercentage: float


# Load trained model
with open("model.pkl", "rb") as file:
    model = pickle.load(file)


@app.get("/")
def root():
    return {
        "message": "TalentMap AI Recommendation Service is running"
    }


@app.post("/predict")
def predict(request: RecommendationRequest):

    features = [[
        request.matchingSkills,
        request.resourceSkillCount,
        request.matchPercentage
    ]]

    prediction = model.predict(features)[0]

    # Probability that the resource is recommended
    probabilities = model.predict_proba(features)[0]
    score = float(probabilities[1])

    return {
        "recommended": bool(prediction),
        "score": score
    }
