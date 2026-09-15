from sklearn.tree import DecisionTreeClassifier
import pickle

# Features:
# [MatchingSkills, ResourceSkillCount, MatchPercentage]

X = [
    [0, 1, 0.0],
    [0, 2, 0.0],
    [1, 2, 0.5],
    [1, 3, 0.333],
    [1, 1, 1.0],
    [2, 2, 1.0],
    [2, 3, 0.667],
    [3, 3, 1.0],
    [2, 4, 0.5],
    [3, 4, 0.75],
]

# 0 = not recommended
# 1 = recommended
y = [
    0,
    0,
    1,
    0,
    1,
    1,
    1,
    1,
    1,
    1
]

# Create Decision Tree model
model = DecisionTreeClassifier(
    max_depth=3,
    random_state=42
)

# Train model
model.fit(X, y)

# Save trained model
with open("model.pkl", "wb") as file:
    pickle.dump(model, file)

print("Model trained successfully.")
print("Model saved as model.pkl")