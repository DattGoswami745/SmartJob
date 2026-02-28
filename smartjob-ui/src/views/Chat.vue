<template>
  <div class="chat-wrapper">
    <div class="container py-4">
      <div class="card chat-card shadow-lg">
        <div class="card-body d-flex flex-column h-100">

          <h4 class="mb-3">AI Assistant</h4>

          <!-- Messages -->
          <div class="chat-messages flex-grow-1 mb-3" ref="chatBox">
            <div
              v-for="(msg, index) in messages"
              :key="index"
              :class="['chat-bubble', msg.role]"
            >
              {{ msg.content }}
            </div>

            <div v-if="loading" class="chat-bubble assistant typing">
              Thinking...
            </div>
          </div>

          <!-- Input -->
          <div class="input-group">
            <input
              v-model="userMessage"
              @keyup.enter="sendMessage"
              class="form-control"
              placeholder="Ask anything..."
            />
            <button
              class="btn send-btn"
              @click="sendMessage"
              :disabled="loading"
            >
              Send
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

  try {
    const response = await sendGeminiMessage(question)

    messages.value.push({
      role: "assistant",
      content: response
    })

    await nextTick()
    chatBox.value.scrollTop = chatBox.value.scrollHeight
  } catch (err) {
    messages.value.push({
      role: "assistant",
      content: "Gemini failed to respond."
    })
  }

  loading.value = false
}
</script>

<style scoped>
.chat-wrapper {
  background-color: var(--bg-main);
  min-height: 100vh;
}

.chat-card {
  background-color: var(--bg-card);
  border: 1px solid var(--border);
  height: 80vh;
}

.chat-messages {
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

.chat-bubble {
  padding: 10px 14px;
  border-radius: 18px;
  margin-bottom: 10px;
  max-width: 75%;
  word-wrap: break-word;
}

.chat-bubble.user {
  background-color: var(--sidebar-hover-bg);
  color: white;
  align-self: flex-end;
  margin-left: auto;
}

.chat-bubble.assistant {
  background-color: var(--recent-bg);
  color: var(--text-primary);
  border: 1px solid var(--border);
}

.typing {
  opacity: 0.7;
  font-style: italic;
}

.send-btn {
  background-color: var(--sidebar-hover-bg);
  color: white;
  border: none;
}
</style>
