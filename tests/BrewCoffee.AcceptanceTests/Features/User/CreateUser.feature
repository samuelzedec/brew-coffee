Feature: Criação de Usuário
Como administrador do sistema
Quero criar novos usuários
Para que possam acessar a plataforma

    Scenario: Criar usuário com sucesso
        Given que não existe um usuário cadastrado
        When eu enviar uma requisição para criar um usuário com os dados válidos
        Then devo receber o status 204

    Scenario: Não deve criar usuário com username duplicado
        Given que existe um usuário cadastrado com username "john_doe"
        When eu enviar uma requisição para criar um usuário com username "john_doe"
        Then devo receber o status 409
        And a mensagem de erro deve ser "O nome de usuário ja está em uso."

    Scenario: Não deve criar usuário com email duplicado
        Given que existe um usuário cadastrado com email "john@email.com"
        When eu enviar uma requisição para criar um usuário com email "john@email.com"
        Then devo receber o status 409
        And a mensagem de erro deve ser "O email já está em uso."