# ShopApi Docker Deployment

## Требования
- Docker Engine 20.10+
- Docker Compose 2.0+

## Структура

```
ShopApi/
├── Dockerfile                  # Образ приложения
├── docker-compose.yml          # Production конфигурация
├── docker-compose.dev.yml      # Development конфигурация
├── .dockerignore              # Исключения для сборки
└── DOCKER.md                  # Этот файл
```

## Быстрый старт

### Production режим

```bash
# Сборка и запуск
docker compose up -d

# Просмотр логов
docker compose logs -f shopapi

# Остановка
docker compose down

# Полная очистка (включая volumes)
docker compose down -v
```

### Development режим

```bash
# Запуск с development конфигурацией
docker compose -f docker-compose.dev.yml up -d

# Пересборка после изменений кода
docker compose -f docker-compose.dev.yml up --build -d

# Просмотр логов
docker-compose -f docker-compose.dev.yml logs -f
```

## Доступные команды

```bash
# Сборка образа без запуска
docker-compose build

# Запуск в фоновом режиме
docker-compose up -d

# Запуск с отображением логов
docker-compose up

# Остановка сервисов
docker-compose stop

# Остановка и удаление контейнеров
docker-compose down

# Просмотр статуса
docker-compose ps

# Просмотр логов конкретного сервиса
docker-compose logs -f shopapi
docker-compose logs -f postgres

# Вход в контейнер
docker-compose exec shopapi bash
docker-compose exec postgres psql -U postgres -d ShopDb
```

## Конфигурация

### Переменные окружения

| Переменная | Значение по умолчанию | Описание |
|------------|----------------------|----------|
| `ASPNETCORE_ENVIRONMENT` | Production | Режим работы приложения |
| `ConnectionStrings__DefaultConnection` | Host=postgres;... | Строка подключения к PostgreSQL |
| `JwtSettings__Secret` | — | Секретный ключ для JWT |
| `JwtSettings__Issuer` | ShopApi | Издатель JWT токенов |
| `JwtSettings__Audience` | ShopApiClient | Аудитория JWT токенов |

### Порты

| Сервис | Внешний порт | Внутренний порт |
|--------|-------------|----------------|
| ShopApi | 5000 | 80 |
| PostgreSQL | 5432 | 5432 |

## Health Check

```bash
# Проверка здоровья контейнеров
docker-compose ps

# Проверка health status PostgreSQL
docker-compose exec postgres pg_isready -U postgres

# Проверка API (изнутри контейнера)
docker-compose exec shopapi curl http://localhost/health
```

## Управление базой данных

```bash
# Подключение к PostgreSQL
docker compose exec postgres psql -U postgres -d ShopDb

# Создание резервной копии
docker-compose exec postgres pg_dump -U postgres ShopDb > backup.sql

# Восстановление из резервной копии
docker-compose exec -T postgres psql -U postgres ShopDb < backup.sql
```

## Troubleshooting

### Проблема: Контейнер сразу останавливается

```bash
# Проверка логов
docker-compose logs shopapi

# Проверка, что PostgreSQL готов
docker-compose logs postgres
```

### Проблема: Не удается подключиться к базе данных

Убедитесь, что PostgreSQL healthy:
```bash
docker-compose ps
```

### Проблема: Порт уже занят

Измените порт в docker-compose.yml:
```yaml
ports:
  - "5001:80"  # вместо 5000:80
```

### Пересборка образа

```bash
# Без кэша
docker-compose build --no-cache

# Пересобрать только shopapi
docker compose build shopapi
```
