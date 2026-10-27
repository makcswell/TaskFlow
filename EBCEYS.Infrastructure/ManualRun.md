# Ручной запуск тестовых сервисов

В репозитории есть два тестовых сервиса:
 * Ebceys.Infrastructure.TestApplication
 * Ebceys.Infrastructure.AuthorizationTestApplication

Их можно запускать чтобы вручную пощупать функционал библиотеки.

Доступ к апи:
 * http://localhost:5033/swagger/index.html (Ebceys.Infrastructure.TestApplication)
 * http://localhost:5067/swagger/index.html (Ebceys.Infrastructure.AuthorizationTestApplication)

Примеры использования можно посмотреть в [интеграционных тестах](./Ebceys.Infrastructure.Tests).

Необходимые зависимости:
 * rabbitmq
 * postgres

Docker compose для запуска зависимостей:
```yaml
services:
  rabbitmq:
    image: rabbitmq:4.2.3-management
    container_name: rabbitmq
    restart: always
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "-q", "ping"]
      interval: 30s
      timeout: 10s
      retries: 5
  db:
    image: postgres:16.3
    container_name: postgres_db
    hostname: postgres-server
    restart: always
    environment:
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: admin
      POSTGRES_DB: my_database
    ports:
      - "8082:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  rabbitmq_data:
  postgres_data:
```