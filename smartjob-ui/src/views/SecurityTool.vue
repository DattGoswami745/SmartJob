<template>
  <div class="security-tool-page d-flex align-items-center justify-content-center min-vh-100 p-4">
    <div class="container" style="max-width: 900px;">
      <!-- Header -->
      <div class="text-center mb-5 animate-in">
        <div class="logo-box mb-3 mx-auto">
          <i class="bi bi-shield-lock-fill text-primary display-4"></i>
        </div>
        <h1 class="fw-bold tracking-tight">Security Configuration Utility</h1>
        <p class="text-muted lead">Standalone tool for encrypting and decrypting system keys</p>
      </div>

      <div class="row g-4">
        <!-- ENCRYPT SECTION -->
        <div class="col-md-6">
          <div class="card shadow-lg border-0 h-100 hover-lift">
            <div class="card-header bg-primary text-white py-3">
              <h4 class="mb-0 fw-bold"><i class="bi bi-lock me-2"></i>Encrypt Plain Text</h4>
            </div>
            <div class="card-body p-4 d-flex flex-column">
              <label class="form-label fw-semibold text-muted small text-uppercase">Input Value</label>
              <textarea 
                v-model="encryptInput" 
                class="form-control custom-textarea mb-3" 
                rows="4" 
                placeholder="Enter sensitive value (e.g., API Key, SMTP Password)..."
              ></textarea>
              
              <button 
                class="btn btn-primary py-2 fw-bold w-100 mb-3" 
                @click="onEncrypt" 
                :disabled="loadingEnc || !encryptInput.trim()"
              >
                <span v-if="loadingEnc" class="spinner-border spinner-border-sm me-2"></span>
                <i v-else class="bi bi-key-fill me-2"></i> Encrypt
              </button>

              <label v-if="encryptOutput" class="form-label fw-semibold text-muted small text-uppercase">Resulting Cipher (Base64)</label>
              <div v-if="encryptOutput" class="result-box p-3 rounded bg-light border mb-2 position-relative">
                <code class="text-break">{{ encryptOutput }}</code>
                <button class="btn btn-sm btn-link copy-btn" @click="copyToClipboard(encryptOutput)">
                  <i class="bi bi-clipboard"></i>
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- DECRYPT SECTION -->
        <div class="col-md-6">
          <div class="card shadow-lg border-0 h-100 hover-lift">
            <div class="card-header bg-dark text-white py-3">
              <h4 class="mb-0 fw-bold"><i class="bi bi-unlock me-2"></i>Decrypt Cipher Text</h4>
            </div>
            <div class="card-body p-4 d-flex flex-column">
              <label class="form-label fw-semibold text-muted small text-uppercase">Encrypted Value</label>
              <textarea 
                v-model="decryptInput" 
                class="form-control custom-textarea mb-3" 
                rows="4" 
                placeholder="Paste the Base64 cipher text here..."
              ></textarea>
              
              <button 
                class="btn btn-dark py-2 fw-bold w-100 mb-3" 
                @click="onDecrypt" 
                :disabled="loadingDec || !decryptInput.trim()"
              >
                <span v-if="loadingDec" class="spinner-border spinner-border-sm me-2"></span>
                <i v-else class="bi bi-key-fill me-2 rotate-180"></i> Decrypt
              </button>

              <label v-if="decryptOutput" class="form-label fw-semibold text-muted small text-uppercase">Decrypted Text</label>
              <div v-if="decryptOutput" class="result-box p-3 rounded bg-success-light border border-success mb-2 position-relative">
                <code class="text-dark text-break">{{ decryptOutput }}</code>
                <button class="btn btn-sm btn-link copy-btn text-dark" @click="copyToClipboard(decryptOutput)">
                  <i class="bi bi-clipboard"></i>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Footer back link -->
      <div class="text-center mt-5">
        <router-link to="/login" class="text-decoration-none text-muted">
          <i class="bi bi-arrow-left me-1"></i> Return to Login
        </router-link>
      </div>
    </div>

    <!-- Removed local toast in favor of centralized one -->
  </div>
</template>

<script setup>
import { ref } from "vue"
import { encryptText, decryptText } from "@/services/api"
import { handleError, handleSuccess } from "@/utils/error-handler"

const encryptInput = ref("")
const encryptOutput = ref("")
const loadingEnc = ref(false)

const decryptInput = ref("")
const decryptOutput = ref("")
const loadingDec = ref(false)

// Removed local toast helper

const onEncrypt = async () => {
  try {
    loadingEnc.value = true
    encryptOutput.value = ""
    const res = await encryptText(encryptInput.value)
    encryptOutput.value = res.result
    handleSuccess("Encryption successful!")
  } catch (err) {
    handleError(err, "Encryption Failed")
  } finally {
    loadingEnc.value = false
  }
}

const onDecrypt = async () => {
  try {
    loadingDec.value = true
    decryptOutput.value = ""
    const res = await decryptText(decryptInput.value)
    decryptOutput.value = res.result
    handleSuccess("Decryption successful!")
  } catch (err) {
    handleError(err, "Decryption Failed")
  } finally {
    loadingDec.value = false
  }
}

const copyToClipboard = (text) => {
  navigator.clipboard.writeText(text)
  handleSuccess("Copied to clipboard!")
}
</script>

<style scoped>
.security-tool-page {
  background: radial-gradient(circle at top right, #f8fafc, #e2e8f0);
  background-attachment: fixed;
}

.logo-box {
  width: 80px;
  height: 80px;
  background: #fff;
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 10px 25px rgba(0,0,0,0.05);
}

.tracking-tight {
  letter-spacing: -0.025em;
  color: #0f172a;
}

.card {
  border-radius: 1.25rem;
  transition: all 0.3s ease;
}

.hover-lift:hover {
  transform: translateY(-8px);
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.15) !important;
}

.custom-textarea {
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  padding: 1rem;
  font-family: 'Monaco', 'Consolas', monospace;
  font-size: 0.9rem;
  resize: none;
}

.custom-textarea:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1);
}

.result-box {
  font-family: 'Monaco', 'Consolas', monospace;
  font-size: 0.85rem;
  max-height: 150px;
  overflow-y: auto;
}

.bg-success-light {
  background-color: #f0fdf4;
}

.copy-btn {
  position: absolute;
  top: 5px;
  right: 5px;
  opacity: 0.6;
}

.copy-btn:hover {
  opacity: 1;
}

.rotate-180 {
  transform: rotate(180deg);
}

.animate-in {
  animation: fadeInDown 0.8s ease-out;
}

@keyframes fadeInDown {
  from { opacity: 0; transform: translateY(-20px); }
  to { opacity: 1; transform: translateY(0); }
}

</style>
