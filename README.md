# 🎬 Movie Database REST API

REST API на **ASP.NET Core 9** для базы данных фильмов.  
База данных: **SQLite** (файл `movies.db`, создаётся автоматически).  
Документация: **Swagger UI** на `http://localhost:5000`.

---

## Структура сущностей и связи

```
Director ──(1:N, SET NULL)──► Movie ◄──(N:M, CASCADE)── Genre
                                │
                          (N:M, CASCADE)
                                │
                              Actor
```

| Связь | Поведение при удалении |
|---|---|
| Удалить **Director** | `DirectorId` в фильмах → **NULL** (SET NULL) |
| Удалить **Movie** | Строки в `MovieGenres` и `MovieActors` → **CASCADE удаление** |
| Удалить **Genre** | Строки в `MovieGenres` → **CASCADE удаление** |
| Удалить **Actor** | Строки в `MovieActors` → **CASCADE удаление** |

---

## Быстрый старт

```bash
cd MovieAPI

# Восстановить пакеты
dotnet restore

# Запустить (БД создастся автоматически)
dotnet run

# Swagger UI откроется на:
# http://localhost:5000
```

---

## JWT Аутентификация

| Действие | Требование |
|---|---|
| GET (чтение) | **Без токена** |
| POST, PUT, PATCH | JWT (любой пользователь) |
| DELETE, создание жанров | JWT с ролью **Admin** |

### Получить токен

```http
POST /api/auth/login
{ "username": "admin", "password": "Admin123" }
```

Вставь токен в Swagger: кнопка **Authorize 🔒** → `Bearer <token>`

---

## Эндпоинты

### Auth
| Метод | URL | Описание |
|---|---|---|
| POST | `/api/auth/register` | Регистрация |
| POST | `/api/auth/login` | Логин → JWT |

### Genres
| Метод | URL | Auth | Описание |
|---|---|---|---|
| GET | `/api/genres` | — | Все жанры |
| GET | `/api/genres/{id}` | — | Жанр по id |
| POST | `/api/genres` | Admin | Создать |
| PUT | `/api/genres/{id}` | Admin | Обновить |
| DELETE | `/api/genres/{id}` | Admin | Удалить (CASCADE) |

### Directors
| Метод | URL | Auth | Описание |
|---|---|---|---|
| GET | `/api/directors` | — | Все режиссёры |
| GET | `/api/directors/{id}` | — | По id |
| POST | `/api/directors` | User+ | Создать |
| PUT | `/api/directors/{id}` | User+ | Обновить |
| DELETE | `/api/directors/{id}` | Admin | Удалить (SET NULL) |

### Actors
| Метод | URL | Auth | Описание |
|---|---|---|---|
| GET | `/api/actors` | — | Все актёры |
| GET | `/api/actors/{id}` | — | По id |
| POST | `/api/actors` | User+ | Создать |
| PUT | `/api/actors/{id}` | User+ | Обновить |
| DELETE | `/api/actors/{id}` | Admin | Удалить (CASCADE) |

### Movies
| Метод | URL | Auth | Описание |
|---|---|---|---|
| GET | `/api/movies` | — | Список (фильтр: year, genre, minRating) |
| GET | `/api/movies/{id}` | — | Полная карточка с жанрами и актёрами |
| POST | `/api/movies` | User+ | Создать с жанрами и кастом |
| PUT | `/api/movies/{id}` | User+ | Полная замена (жанры + каст) |
| PATCH | `/api/movies/{id}/rating` | User+ | Обновить только рейтинг |
| DELETE | `/api/movies/{id}` | Admin | Удалить (CASCADE) |

---

## Тестирование через HTTP-клиент

Файл `requests.http` содержит 25 готовых запросов.

- **VS Code**: установи расширение [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)
- **JetBrains Rider / IDEA**: встроенная поддержка `.http` файлов

Логин сделан с `# @name login`, токен автоматически подставляется в следующие запросы через `{{adminToken}}`.

---

## Тестовые данные (seed)

**Режиссёры:** Christopher Nolan, Quentin Tarantino, Luc Besson, Hayao Miyazaki  
**Актёры:** DiCaprio, Cillian Murphy, Travolta, Uma Thurman, Jovovich, Bruce Willis  
**Фильмы:** Inception, Oppenheimer, Pulp Fiction, The Fifth Element, Spirited Away  
**Жанры:** Action, Drama, Sci-Fi, Crime, Animation  
**Пользователь:** `admin` / `Admin123`
