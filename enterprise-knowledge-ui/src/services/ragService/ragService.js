
import axios from "axios";

const API_BASE_URL = "https://localhost:7121/api";

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

export const askRagQuestion = async (question) => {
  const response = await apiClient.post("/rag/ask", {
    question: question,
    topK: 5,
  });

  return response.data;
};
