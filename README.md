# 🛒 OrderFlow - E-commerce System

Este projeto simula um sistema de pedidos para uma loja virtual. O sistema envolve a integração entre múltiplas APIs, como a OrderAPI e a ProductAPI, com funcionalidades voltadas para o processamento de pedidos, pagamento, verificação de estoque, cálculos de distância e envio de notificações ao usuário.

## 🚀 Funcionalidades

### API de Pedidos
- **Criar novo pedido:** Cria um novo pedido, associando um endereço de entrega e gerando um link para o pagamento.
- **Gerenciar endereços:** Adicionar, atualizar e excluir endereços associados ao usuário.
- **Gerenciar pedidos:** Consultar todos os pedidos, atualizar dados de um pedido ainda não processado e excluir pedidos.
- **Cálculo de distância:** Utiliza a API de Google Matrix para calcular a distância aproximada entre o endereço do usuário e o endereço da loja.
- **Integração com RabbitMQ:** Ao gerar um pedido, envia uma mensagem para a API de Produtos via RabbitMQ para verificar e atualizar o estoque.
- **Notificação por email:** Envia e-mails ao usuário, notificando sobre o sucesso ou falha do processamento do pedido.
- **Envio de e-mail de atualização de entrega:** Quando o prazo de entrega é atingido, a API de Pedidos atualiza o status da entrega para "shipped" e envia um e-mail ao usuário.

### API de Produtos
- **Gerenciar produtos e estoque:** Atualizar estoque, adicionar novos produtos, ver todos os produtos existentes e filtrar por nome, chave ou ID.
- **Verificação de estoque:** Verifica se há estoque suficiente para atender a um pedido.
- **Atualização do estoque:** Atualiza o estoque e responde à API de Pedidos via RabbitMQ.

### Integração de Pagamento
- **Stripe:** Ao gerar um pedido, é criado um link para pagamento no Stripe. O pagamento é realizado e, em seguida, uma mensagem é enviada para a API de Produtos, o pedido só é confirmado após a resposta da API de Produtos.
- **RabbitMQ:** O RabbitMQ é utilizado para comunicação entre as APIs de Pedidos e Produtos, garantindo a integridade e o processamento correto do pedido.

### Quartz
- **Atualização de status de entrega:** O Quartz é utilizado para monitorar o prazo de entrega e, ao ser atingido, atualiza o status de "shipped" e envia um e-mail para o usuário.

## 🛠 Ferramentas Utilizadas
- **.NET Core:** Desenvolvimento das APIs.
- **Docker:** Banco de dados PostgreSQL e RabbitMQ em containers.
- **RabbitMQ:** Comunicação assíncrona entre APIs.
- **Stripe API:** Integração para pagamentos.
- **Google Matrix API:** Cálculo de distância entre endereços.
- **Quartz Scheduler:** Gerenciamento de agendamentos de entregas.
- **SMTP:** Envio de e-mails de notificação.

## 📚 O Que Foi Aprendido

- **Integração de sistemas:** A integração de múltiplas APIs e sistemas (Stripe, RabbitMQ, Quartz) foi fundamental para garantir a fluidez no processamento de pedidos.
- **Comunicação assíncrona:** O uso de RabbitMQ para troca de mensagens assíncronas entre sistemas permitiu desacoplar as diferentes APIs e tornar o sistema mais escalável.

## 🏃‍♂️ Como Rodar o Projeto

### Pré-requisitos

Antes de rodar o projeto, você precisa ter as seguintes ferramentas:

- .NET Core SDK

- Docker

- Chave da API do Google e Stripe

 

### Passo a Passo para Rodar o Projeto

1. **Clonar o repositório**

   ```bash
   git clone [https://github.com/rayxves/OrderFlow.git]
   ```

 

2. **Subir o ambiente Docker**

   Certifique-se de ter o Docker rodando em sua máquina. Em seguida, crie e inicie os containers necessários para o banco de dados PostgreSQL e RabbitMQ. Deixei um esquelo pronto pra isso.

   ```bash
   docker-compose up -d
   ```

 

  3. **Configurar a aplicação**:
Ajuste as credenciais no arquivo appsettings.json. Em um esqueleto deixei tudo que é necessário para rodar o projeto.


  
  5. **Rodar o projeto**:
   Dentro de cada diretório da API, execute:

     ```bash
     dotnet watch run
     ```
     
   Depois que uma aba for aberta, adicione /swagger para testar e visualizar as API's.


  
  6. **Verificar os logs**: Se achar necessário, utilize logs para acompanhar o status da aplicação, especialmente para ver as interações com RabbitMQ e as respostas da API.
     

---
