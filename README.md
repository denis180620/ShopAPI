# ShopApi

REST API для интернет-магазина с аутентификацией пользователей, управлением заказами, товарами и категориями.

## Технологии

- **ASP.NET Core** — Web API фреймворк
- **PostgreSQL** — база данных
- **Entity Framework Core** — ORM
- **JWT + Refresh Tokens** — аутентификация
- **ASP.NET Identity** — управление пользователями и ролями
- **Swagger** — документация API

## Установка и запуск

### Требования

- .NET 8.0 SDK
- PostgreSQL 14+

### Настройка

1. Клонируйте репозиторий и перейдите в папку проекта:
```bash
cd ShopApi
```

2. Настройте соединение с базой данных в `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ShopDb;Username=postgres;Password=your_password"
  }
}
```

3. Настройте JWT в `appsettings.json`:
```json
{
  "JwtSettings": {
    "Secret": "your-super-secret-key-at-least-32-characters-long",
    "Issuer": "ShopApi",
    "Audience": "ShopApiClient",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

4. Настройте Email для подтверждения регистрации (опционально):
```json
{
  "Email:SmtpServer": "smtp.yandex.com",
  "Email:Port": "587",
  "Email:Username": "your-email@yandex.ru",
  "Email:Password": "your-password",
  "Email:From": "your-email@yandex.ru"
}
```

5. Примените миграции для создания базы данных:
```bash
dotnet ef database update
```

### Запуск

```bash
dotnet run
```

API будет доступен по адресу: `https://localhost:5001`

Документация Swagger: `https://localhost:5001/swagger`

---

## API Эндпоинты

### Авторизация (`/api/ControllerAuth`)

| Метод | Эндпоинт | Описание | Auth |
|-------|----------|----------|------|
| POST | `/register` | Регистрация нового пользователя | ❌ |
| POST | `/login` | Вход в систему (возвращает JWT + RefreshToken в cookie) | ❌ |
| POST | `/logout` | Выход из системы | ✅ |
| POST | `/me` | Получение данных текущего пользователя | ✅ |
| POST | `/forgot` | Отправка кода восстановления пароля на email | ❌ |
| POST | `/confirm` | Подтверждение email | ❌ |
| POST | `/reset` | Сброс пароля | ❌ |

**Пример регистрации:**
```json
POST /api/ControllerAuth/register
{
  "firstName": "Иван",
  "lastName": "Иванов",
  "email": "user@example.com",
  "password": "Password123"
}
```

**Пример входа:**
```json
POST /api/ControllerAuth/login
{
  "email": "user@example.com",
  "password": "Password123"
}
```

---

### Заказы (`/api/Order`)

| Метод | Эндпоинт | Описание | Auth | Роли |
|-------|----------|----------|------|------|
| POST | `/` | Создание заказа | ❌ | - |
| PUT | `/` | Добавление товаров в заказ | ❌ | - |
| GET | `/{id}` | Получение заказа по ID | ❌ | - |
| GET | `/` | Получение всех заказов | ✅ | Admin, Manager |
| GET | `/status?status=Pending` | Получение заказов по статусу | ✅ | Admin, Manager |
| PUT | `/` (body: Order) | Изменение заказа | ✅ | Admin, Manager |
| GET | `/` | Получение заказов текущего пользователя | ✅ | User |
| DELETE | `/{id}` | Удаление заказа | ✅ | Admin, Manager |
| GET | `/{id}` (BuyOrder) | Оплата заказа | ✅ | User |

**Пример создания заказа:**
```json
POST /api/Order
{
  "orderItems": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 2
    }
  ]
}
```

---

### Товары (`/api/ControllerProduct`)

| Метод | Эндпоинт | Описание | Auth | Роли |
|-------|----------|----------|------|------|
| POST | `/` | Создание товара | ✅ | Admin, Manager |
| PUT | `/` | Обновление товара | ✅ | Admin, Manager |
| PUT | `/{Id}/{quantity}` | Добавление количества на склад | ✅ | Admin, Manager |
| GET | `/` | Получение товаров (для админа) | ✅ | Admin, Manager |
| GET | `/{Id}` | Получение товара по ID | ✅ | Admin, Manager |
| DELETE | `/{Id}` | Удаление товара | ✅ | Admin, Manager |

**Пример создания товара:**
```json
POST /api/ControllerProduct
{
  "name": "Товар",
  "description": "Описание товара",
  "price": 1000,
  "stock": 50,
  "categoryId": 1
}
```

---

### Категории (`/api/ControllerCategory`)

| Метод | Эндпоинт | Описание | Auth | Роли |
|-------|----------|----------|------|------|
| POST | `/` | Создание категории | ✅ | Admin |
| PUT | `/` | Обновление категории | ✅ | Admin |
| GET | `/{id}` | Получение категории по ID | ❌ | - |
| GET | `/` | Получение всех категорий | ❌ | - |
| DELETE | `/{id}` | Удаление категории | ✅ | Admin |
| GET | `/{name}` (дочерние) | Получение дочерних категорий | ✅ | Admin |

**Пример создания категории:**
```json
POST /api/ControllerCategory
{
  "name": "Электроника",
  "description": "Электронные устройства",
  "parentCategoryId": null
}
```

---

## Роли пользователей

- **User** — обычный клиент (может создавать заказы, просматривать свои заказы)
- **Manager** — менеджер (доступ к управлению заказами и товарами)
- **Admin** — администратор (полный доступ ко всем ресурсам)

## Использование API

### Авторизация

Большинство эндпоинтов требуют JWT токен. Передайте его в заголовке:

```
Authorization: Bearer <your-jwt-token>
```

RefreshToken автоматически сохраняется в HTTP-only cookie при логине.

### Статусы ответов

Все эндпоинты возвращают ответ в формате:

```json
{
  "isSuccess": true,
  "message": "Успешно",
  "data": { ... },
  "statusCode": 200
}
```

---

## Лицензия

MIT
