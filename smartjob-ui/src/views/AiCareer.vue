<template>
  <div class="resume-wrapper">
    <div class="container py-5">
      <div class="text-center mb-5">
        <h1 class="fw-bold gradient-text">AI Resume Builder</h1>
        <p class="text-muted">Drag & drop AI suggestions to craft your professional resume</p>
      </div>

      <div class="row g-4">

        <!-- LEFT SIDE : RESUME TEMPLATE -->
        <div class="col-lg-7">
          <div class="card resume-card shadow-lg border-0">
            <div class="card-body p-4">

              <div class="row g-3">
                <div class="col-md-6">
                  <label class="form-label fw-semibold">Full Name</label>
                  <input v-model="resume.fullName" class="form-control stylish-input"/>
                </div>

                <div class="col-md-6">
                  <label class="form-label fw-semibold">Email</label>
                  <input v-model="resume.email" class="form-control stylish-input"/>
                </div>
              </div>

              <!-- SUMMARY -->
              <div class="section mt-4">
                <h5 class="section-title">Professional Summary</h5>
                <draggable
                  v-model="resume.summary"
                  :group="{ name:'summary', pull:true, put:true }"
                  item-key="id"
                  class="drop-zone">
                  <template #item="{ element, index }">
                    <div class="resume-item">
                      <textarea v-model="element.text" class="form-control stylish-textarea"></textarea>
                      <button class="remove-btn" @click="removeItem('summary',index)">×</button>
                    </div>
                  </template>
                </draggable>
              </div>

              <!-- SKILLS -->
              <div class="section mt-4">
                <h5 class="section-title">Skills</h5>
                <draggable
                  v-model="resume.skills"
                  :group="{ name:'skills', pull:true, put:true }"
                  item-key="id"
                  class="drop-zone">
                  <template #item="{ element, index }">
                    <div class="resume-item">
                      <textarea v-model="element.text" class="form-control stylish-textarea"></textarea>
                      <button class="remove-btn" @click="removeItem('skills',index)">×</button>
                    </div>
                  </template>
                </draggable>
              </div>

              <!-- EXPERIENCE -->
              <div class="section mt-4">
                <h5 class="section-title">Experience</h5>
                <draggable
                  v-model="resume.experience"
                  :group="{ name:'experience', pull:true, put:true }"
                  item-key="id"
                  class="drop-zone">
                  <template #item="{ element, index }">
                    <div class="resume-item">
                      <textarea v-model="element.text" class="form-control stylish-textarea"></textarea>
                      <button class="remove-btn" @click="removeItem('experience',index)">×</button>
                    </div>
                  </template>
                </draggable>
              </div>

              <button class="btn download-btn w-100 mt-4" @click="handleDownload">
                Download Resume
              </button>

            </div>
          </div>
        </div>

        <!-- RIGHT SIDE : AI SUGGESTIONS -->
        <div class="col-lg-5">
          <div class="card suggestion-card shadow-lg border-0">
            <div class="card-body p-4">
              <h5 class="fw-bold mb-3">AI Suggestions</h5>

              <div v-if="loading" class="text-center py-4">
                <div class="spinner-border text-primary"></div>
                <p class="mt-2 text-muted">Generating suggestions...</p>
              </div>

              <div v-else>
                <div class="suggestion-section">
                  <h6>Summary</h6>
                  <draggable
                    v-model="availableItems.summary"
                    :group="{ name:'summary', pull:'clone', put:false }"
                    :clone="cloneItem"
                    :sort="false"
                    item-key="id">
                    <template #item="{ element }">
                      <div class="suggestion-item">{{ element.text }}</div>
                    </template>
                  </draggable>
                </div>

                <div class="suggestion-section mt-4">
                  <h6>Skills</h6>
                  <draggable
                    v-model="availableItems.skills"
                    :group="{ name:'skills', pull:'clone', put:false }"
                    :clone="cloneItem"
                    :sort="false"
                    item-key="id">
                    <template #item="{ element }">
                      <div class="suggestion-item">{{ element.text }}</div>
                    </template>
                  </draggable>
                </div>

                <div class="suggestion-section mt-4">
                  <h6>Experience</h6>
                  <draggable
                    v-model="availableItems.experience"
                    :group="{ name:'experience', pull:'clone', put:false }"
                    :clone="cloneItem"
                    :sort="false"
                    item-key="id">
                    <template #item="{ element }">
                      <div class="suggestion-item">{{ element.text }}</div>
                    </template>
                  </draggable>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import draggable from "vuedraggable"
import { getResumeSuggestions, downloadResumeFile } from "../services/api"

const resume = ref({ fullName:"", email:"", summary:[], skills:[], experience:[] })
const availableItems = ref({ summary:[], skills:[], experience:[] })
const loading = ref(true)

function cloneItem(item){
  return { ...item, id: Date.now() + Math.random() }
}

onMounted(async () => {
  try{
    const data = await getResumeSuggestions()
    let id = 1

    resume.value.fullName = data.fullName || ""
    resume.value.email = data.email || ""

    availableItems.value.summary = (data.summary || []).map(t => ({ id:id++, text:t }))
    availableItems.value.skills = (data.skills || []).map(t => ({ id:id++, text:t }))
    availableItems.value.experience = (data.experience || []).map(t => ({ id:id++, text:t }))
  }
  catch(err){
    alert("AI failed to generate suggestions.")
  }
  finally{
    loading.value = false
  }
})

function removeItem(section,index){
  const item = resume.value[section].splice(index,1)[0]
  availableItems.value[section].push(item)
}

async function handleDownload(){
  try{
    const blob = await downloadResumeFile(resume.value)
    const link = document.createElement("a")
    link.href = window.URL.createObjectURL(blob)
    link.download = "Resume.docx"
    link.click()
  }catch(err){
    alert("Download failed: " + err.message)
  }
}
</script>

<style scoped>
.resume-wrapper{
  background: linear-gradient(135deg,#eef2ff,#f8fafc);
  min-height:100vh;
}

.gradient-text{
  background: linear-gradient(90deg,#4f46e5,#06b6d4);
  -webkit-background-clip:text;
  -webkit-text-fill-color:transparent;
}

.resume-card, .suggestion-card{
  border-radius:20px;
}

.section-title{
  font-weight:600;
  margin-bottom:10px;
}

.drop-zone{
  min-height:90px;
  padding:15px;
  border:2px dashed #cbd5e1;
  border-radius:12px;
  background:white;
  transition:all 0.3s;
}

.drop-zone:hover{
  border-color:#6366f1;
  background:#f8fafc;
}

.resume-item{
  position:relative;
  margin-bottom:10px;
}

.stylish-textarea{
  border-radius:10px;
  resize:none;
}

.stylish-input{
  border-radius:10px;
}

.remove-btn{
  position:absolute;
  right:8px;
  top:8px;
  border:none;
  background:#ef4444;
  color:white;
  width:22px;
  height:22px;
  border-radius:50%;
  font-size:14px;
  cursor:pointer;
}

.suggestion-item{
  background:#f1f5f9;
  padding:10px;
  border-radius:10px;
  margin-bottom:8px;
  cursor:grab;
  transition:all 0.2s;
}

.suggestion-item:hover{
  background:#e0e7ff;
  transform:translateY(-2px);
}

.download-btn{
  background:linear-gradient(90deg,#4f46e5,#06b6d4);
  border:none;
  border-radius:12px;
  font-weight:600;
  padding:12px;
  color:white;
  transition:0.3s;
}

.download-btn:hover{
  transform:scale(1.02);
  opacity:0.9;
}
</style>
