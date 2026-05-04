# Contexto para IAs - Sistema Solar

## Visão Geral
Este é um projeto de SaaS B2B para integradores de energia solar. O vault de documentação está em `C:\Users\teste\Documents\SecondBrain\sistema-solar` e o código em `C:\Users\teste\Documents\sistema-solar`.

## Stack Tecnológica
- **Backend**: .NET 8, Clean Architecture, REST API
- **Frontend**: React + TypeScript + Vite + TailwindCSS (ainda não criado)
- **Database**: PostgreSQL com EF Core
- **Infra**: Docker, RabbitMQ, Azure
- **Auth**: JWT com claims (tenant_id, user_id, role)

## Documentação de Referência
- `Sistema Solar - Contexto do Projeto.md` - Contexto geral
- `Arquitetura - Sistema Solar.md` - Decisões técnicas
- `Sistema Solar — Backlog e Entregas.md` - Backlog com status atualizado
- `Cenarios de Seguranca.md` - OWASP Top10
- `Cenarios de Testes.md` - Estratégia de testes

## Status Atual do Projeto (até 04/05/2026)
### Concluído:
- **EP-01 (Fundação)**: 100% - Clean Architecture, Docker, EF Core, Migrations, Health, Swagger
- **EP-02 (Leads)**: Backend 100% - Entities, Commands, Queries, Repository, Controller, Validações
- **EP-08 (Auth)**: Infraestrutura pronta - JWT configurado, Tenant/User entities, Global Query Filter

### Pendências:
- **US-002**: Frontend não criado
- **EP-08**: Falta AuthController, login/registro (usa DevCurrentUserService mock)
- **EP-03 a EP-09**: Ainda não iniciados

## 🔴 FLUXO OBRIGATÓRIO AO FINAL DE CADA ENTREGA

**Toda tarefa concluída DEVE seguir este fluxo:**

1. **Atualizar documentação**: Marcar itens como `[x]` no arquivo `Sistema Solar — Backlog e Entregas.md` (no vault SecondBrain e no repo)

2. **Criar branch**: `git checkout -b feature/nome-da-feature` ou `fix/nome` ou `docs/nome`

3. **Commits padronizados** (Conventional Commits):
   - `feat:` nova funcionalidade
   - `fix:` correção de bug
   - `docs:` documentação
   - `refactor:` refatoração
   - `test:` testes
   - Exemplo: `feat(leads): implement create lead endpoint`

4. **Push da branch**: `git push -u origin nome-da-branch`

5. **Abrir Pull Request**:
   - Usar `gh pr create`
   - Título descritivo
   - Descrever o que foi feito
   - Referenciar issues/seções do backlog
   - Solicitar revisão

6. **Atualizar este documento** se houver mudanças de contexto

## Padrão de Branches
- `main` - produção
- `develop` - desenvolvimento
- `feature/*` - novas funcionalidades
- `fix/*` - correções
- `docs/*` - documentação

## Comandos Git Importantes
```bash
# Criar branch
git checkout -b feature/nome-da-feature

# Commit com padrão
git commit -m "feat(ep-02): implement lead listing endpoint"

# Push e criar PR
git push -u origin feature/nome-da-feature
gh pr create --title "feat: implement EP-02 leads" --body "Implementa..."
```

## Observações para IAs
- Sempre ler este arquivo no início de cada sessão
- Nunca fazer commit direto na main
- Sempre abrir PR ao finalizar tarefa
- Atualizar o backlog com status real do que foi feito
- O DevCurrentUserService é temporário até EP-08 ser concluído
- Multi-tenancy já implementado via Global Query Filter no SolarDbContext
