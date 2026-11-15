
import { useState } from "react";
import "./App.css";
import { askRagQuestion } from "./services/ragService/ragService.js";
import ReactMarkdown from "react-markdown";

function App() {
  const [messages, setMessages] = useState([
    {
      role: "assistant",
      content:
        "Hello! I'm your Enterprise Knowledge Assistant. Ask me anything about your uploaded documents.",
    },
  ]);

  const [question, setQuestion] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSend = async () => {
    const userQuestion = question.trim();

    if (!userQuestion || loading) return;

    setMessages((previous) => [
      ...previous,
      { role: "user", content: userQuestion },
    ]);

    setQuestion("");
    setLoading(true);

    try {
      const result = await askRagQuestion(userQuestion);

      setMessages((previous) => [
        ...previous,
        {
          role: "assistant",
          content: result.answer,
          sources: result.sources || [],
        },
      ]);
    } catch (error) {
      console.error("RAG API error:", error);

      setMessages((previous) => [
        ...previous,
        {
          role: "assistant",
          content:
            "Sorry, I couldn't connect to the RAG service. Please check that the API is running.",
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="app-container">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-icon">✦</span>
          <h2>Knowledge AI</h2>
        </div>

        <button
          className="new-chat"
          onClick={() =>
            setMessages([
              {
                role: "assistant",
                content: "Hello! How can I help you today?",
              },
            ])
          }
        >
          + New Chat
        </button>

        <div className="sidebar-footer">
          Enterprise RAG POC
        </div>
      </aside>

      <main className="main-content">
        <header className="topbar">
          <div>
            <h2>Enterprise Knowledge Assistant</h2>
            <p>Ask questions from your uploaded documents</p>
          </div>
          <span className="status">● AI Ready</span>
        </header>

        <section className="chat-area">
          {messages.map((message, index) => (
            <div
              key={index}
              className={`message-row ${message.role}`}
            >
              <div className="avatar">
                {message.role === "assistant" ? "AI" : "You"}
              </div>

              <div className="message-content">
                <div className="message-bubble">
                  <ReactMarkdown>
                    {message.content}
                  </ReactMarkdown>
                </div>

                {message.sources?.length > 0 && (
                  <div className="sources">
                    <strong>Sources</strong>

                    {message.sources.map((source, sourceIndex) => (
                      <div key={sourceIndex} className="source-item">
                        📄 {source.fileName} — Chunk {source.chunkIndex}
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          ))}

          {loading && (
            <div className="loading-message">
              AI is searching your documents and preparing an answer...
            </div>
          )}
        </section>

        <div className="input-area">
          <div className="input-wrapper">
            <input
              type="text"
              placeholder="Ask a question about your documents..."
              value={question}
              disabled={loading}
              onChange={(e) => setQuestion(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") handleSend();
              }}
            />

            <button onClick={handleSend} disabled={loading || !question.trim()}>
              {loading ? "..." : "Send ↑"}
            </button>
          </div>

          <p>
            AI-generated answers are grounded in your uploaded documents.
          </p>
        </div>
      </main>
    </div>
  );
}

export default App;
