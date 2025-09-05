# 🗂️ TaskFlow  
> Sistema de Kanban Multiplataforma com Chat em Tempo Real  

---

## 📖 Sobre o Projeto  

O **TaskFlow** é um **sistema de Kanban digital e multiplataforma** que une o **gerenciamento de tarefas** com a **comunicação em tempo real**.  
Seu diferencial é oferecer **quadros, cartões e chat integrado** em um só ambiente, tornando o fluxo de trabalho mais ágil e colaborativo.  

🔹 **Plataformas:** Web e Android  
🔹 **Tecnologias:** React.js, TypeScript, Kotlin, .NET, PostgreSQL, Redis, Docker  

---

## 🚀 Funcionalidades  

- 📋 **Gerenciamento de Quadros:** criar, editar e excluir quadros  
- 🗂 **Gerenciamento de Cartões:** criar, editar, mover e excluir tarefas  
- 💬 **Chat em Tempo Real:** mensagens diretas e em grupo  
- 🔔 **Notificações:** para novas mensagens e movimentações de tarefas  
- 👥 **Controle de Usuários e Permissões:** autenticação segura e hierarquia  

---

## 🏗️ Arquitetura  

```mermaid
---
config:
  layout: elk
---
flowchart TD
 subgraph Clients["Clients"]
        A["React + TypeScript Web"]
        B["Kotlin Android"]
  end
 subgraph Infra["VPS"]
        C[".NET Core API"]
        D[("PostgreSQL")]
        E[("Redis")]
        F["WebSockets / Socket.io"]
  end
    A --> C
    B --> C
    C --> D & E & F
