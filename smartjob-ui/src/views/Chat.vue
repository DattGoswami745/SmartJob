<template>
  <div class="user-dashboard-wrapper chat-wrapper">
    <div class="container py-4 h-100">
      <div class="card premium-table-card h-100 d-flex flex-column shadow-lg border-0 overflow-hidden">
        
        <!-- Header -->
        <div class="chat-header p-4 border-bottom d-flex align-items-center gap-3">
          <div class="icon-box ai-icon">
            <Sparkles class="text-primary" size="24" />
          </div>
          <div>
            <h4 class="mb-0 fw-bold gradient-text">AI Job Assistant</h4>
            <p class="text-muted small m-0">Ask me anything about your career, resume, or jobs!</p>
          </div>
        </div>

        <!-- Messages Area -->
        <div class="chat-messages flex-grow-1 p-4" ref="chatBox">
          
          <div v-if="messages.length === 0" class="text-center text-muted mt-5 py-5 d-flex flex-column align-items-center">
            <MessageSquare class="opacity-25 mb-3" size="48" />
            <h5>No messages yet</h5>
            <p class="small">Say hello or ask a question to start the conversation.</p>
          </div>

          <div
            v-for="(msg, index) in messages"
            :key="index"
            :class="['d-flex mb-4', msg.role === 'user' ? 'justify-content-end' : 'justify-content-start']"
          >
            <div :class="['chat-bubble shadow-sm', msg.role]">
              <div v-if="msg.role === 'assistant'" class="d-flex align-items-center gap-2 mb-1 text-primary fw-bold small">
                <Sparkles size="14" /> AI Assistant
              </div>
              <div v-else class="d-flex align-items-center justify-content-end gap-2 mb-1 text-white fw-bold small opacity-75">
                <User size="14" /> You
              </div>
              <div class="message-text lh-base">{{ msg.content }}</div>
            </div>
          </div>

          <!-- Typing Indicator -->
          <div v-if="loading" class="d-flex mb-4 justify-content-start">
            <div class="chat-bubble assistant typing shadow-sm">
              <div class="typing-dots d-flex gap-1 align-items-center p-2">
                <span></span><span></span><span></span>
              </div>
            </div>
          </div>

        </div>

        <!-- Input Area -->
        <div class="chat-input-area p-3 border-top bg-light">
          <div class="input-group premium-input-group">
            <input
              v-model="userMessage"
              @keyup.enter="sendMessage"
              class="form-control premium-input border-0 shadow-none ps-4"
              placeholder="Type your message here..."
              :disabled="loading"
            />
            <button
              class="btn btn-primary-gradient px-4 d-flex align-items-center gap-2"
              @click="sendMessage"
              :disabled="loading || !userMessage.trim()"
            >
              <Send size="18" /> <span class="d-none d-sm-inline">Send</span>
            </button>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, nextTick } from "vue"
import { sendGeminiMessage } from "../services/api"
import { Sparkles, Send, User, MessageSquare } from "lucide-vue-next"

const userMessage = ref("")
const messages = ref([])
const loading = ref(false)
const chatBox = ref(null)

async function sendMessage() {
  if (!userMessage.value.trim() || loading.value) return

  const question = userMessage.value

  messages.value.push({ role: "user", content: question })
  userMessage.value = ""
  loading.value = true
  scrollToBottom()

  try {
    const response = await sendGeminiMessage(question)

    messages.value.push({
      role: "assistant",
      content: response
    })
    scrollToBottom()
  } catch (err) {
    messages.value.push({
      role: "assistant",
      content: "Gemini failed to respond. Please try again."
    })
    scrollToBottom()
  }

  loading.value = false
}

async function scrollToBottom() {
  await nextTick()
  if (chatBox.value) {
    chatBox.value.scrollTo({
      top: chatBox.value.scrollHeight,
      behavior: "smooth"
    })
  }
}
</script>

<style scoped>
<style scoped>
.user-dashboard-wrapper {
  padding: 0;
  height: calc(100vh - var(--header-height, 60px) - 2rem);
}

@media (max-width: 991.98px) {
  .user-dashboard-wrapper {
     height: calc(100vh - var(--header-height, 60px) - 1rem);
  }
}

.premium-table-card {
  background: var(--bg-card);
  border-radius: 20px;
  height: 100%;
}

.chat-header {
  background: var(--bg-card);
  z-index: 10;
}

.icon-box.ai-icon {
  width: 48px;
  height: 48px;
  background: rgba(59, 130, 246, 0.1);
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px dashed rgba(59, 130, 246, 0.3);
}

.gradient-text {
  background: linear-gradient(90deg, #3b82f6, #8b5cf6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

/* Scrollbar */
.chat-messages {
  overflow-y: auto;
  scroll-behavior: smooth;
  background-color: var(--bg-main);
}
.chat-messages::-webkit-scrollbar {
  width: 6px;
}
.chat-messages::-webkit-scrollbar-thumb {
  background-color: var(--border);
  border-radius: 10px;
}

.chat-bubble {
  padding: 12px 16px;
  border-radius: 18px;
  max-width: 85%;
  position: relative;
  font-size: 0.95rem;
}

@media (max-width: 575.98px) {
  .chat-bubble {
    max-width: 95%;
  }
}

.chat-bubble.user {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: #ffffff;
  border-bottom-right-radius: 4px;
}

.chat-bubble.assistant {
  background: var(--bg-card);
  color: var(--text-main);
  border: 1px solid var(--border);
  border-bottom-left-radius: 4px;
}

.message-text {
  white-space: pre-wrap;
}

/* Typing Dots Animation */
.typing-dots span {
  width: 8px;
  height: 8px;
  background-color: #cbd5e1;
  border-radius: 50%;
  display: inline-block;
  animation: typing 1.4s infinite ease-in-out both;
}
.typing-dots span:nth-child(1) { animation-delay: -0.32s; }
.typing-dots span:nth-child(2) { animation-delay: -0.16s; }
@keyframes typing {
  0%, 80%, 100% { transform: scale(0); }
  40% { transform: scale(1); }
}

/* Input Area */
.chat-input-area {
  background-color: var(--bg-card);
  z-index: 10;
}

.premium-input-group {
  background: #ffffff;
  border: 1px solid var(--border);
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
  transition: all 0.3s;
}
.premium-input-group:focus-within {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15);
}

.premium-input {
  background: transparent;
  padding: 16px;
  font-size: 1rem;
}
.premium-input:focus {
  background: transparent;
}

.btn-primary-gradient {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: white;
  border: none;
  font-weight: 600;
  border-radius: 12px;
  margin: 6px;
  transition: all 0.3s;
}
.btn-primary-gradient:hover:not(:disabled) {
  transform: scale(1.02);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
  color: white;
}
.btn-primary-gradient:disabled {
  opacity: 0.6;
}
</style>
