import express from "express";
import { Login, ObtainToken, Register, ValidateToken } from "./auth.js";
import { editBook, newBook, searchBooks, getBooksByCategory } from "./books.js"; 
import { 
  createCategory, 
  getAllCategories, 
  editCategory as editCategoryFunc, 
  deleteCategory as deleteCategoryFunc, 
  getCategoryStats,
  addCategoryToBook 
} from "./categories.js"; 
import { Book, Category } from "./models.js"; 

const app = express();
export default app;
app.use(express.json());

app.use(async (req, res, next) => {
  try {
    let token;
    if (req.query.token) {
      token = req.query.token;
    } else if (req.body.token) {
      token = req.body.token;
    } else if (req.headers.authorization && req.headers.authorization.startsWith('Bearer ')) {
      token = req.headers.authorization.split(' ')[1];
    }

    if (token) {
      res.locals.user = await ValidateToken(token);
    } else {
      res.locals.user = null;
    }
  } catch {
    res.locals.user = null;
  }
  next();
});

function APIError(res, msg, statusCode = 400) {
  res.status(statusCode).json({ msg: msg });
}

function LoggedInOnly(req, res, next) {
  if (res.locals.user) next();
  else APIError(res, "You must log in to do that.", 401);
}

function LoggedOutOnly(req, res, next) {
  if (res.locals.user) APIError(res, "You're already logged in.");
  else next();
}

app.post("/login", LoggedOutOnly, async (req, res) => {
  const result = await Login(req.body.name, req.body.password);
  if (typeof result == "string") {
    APIError(res, result);
  } else {
    res.json({
      token: ObtainToken(result),
    });
  }
});

app.post("/register", LoggedOutOnly, async (req, res) => {
  const result = await Register(req.body.name, req.body.password);
  if (typeof result == "string") {
    APIError(res, result);
  } else {
    res.json({
      token: ObtainToken(result),
    });
  }
});

app.get("/me", LoggedInOnly, async (req, res) => {
  const { name, createdAt } = res.locals.user;
  res.json({ name: name, createdAt: createdAt });
});

app.post("/api/categories", LoggedInOnly, async (req, res) => {
  const { name } = req.body;
  if (!name) return APIError(res, "Category name is required.");
  const result = await createCategory(name);
  if (typeof result === "string") {
    APIError(res, result);
  } else {
    res.status(201).json(result);
  }
});

app.get("/api/categories", async (req, res) => {
  const categories = await getAllCategories();
  res.json(categories);
});

app.put("/api/books/:bookId/category", LoggedInOnly, async (req, res) => {
    const { bookId } = req.params;
    const { categoryId } = req.body;

    if (!categoryId) {
        return APIError(res, "categoryId is required in the request body.");
    }
    if (isNaN(parseInt(bookId)) || isNaN(parseInt(categoryId))) {
        return APIError(res, "Invalid bookId or categoryId.");
    }

    const result = await addCategoryToBook(parseInt(bookId), parseInt(categoryId));
    if (typeof result === 'string') {
        APIError(res, result);
    } else {
        res.json(result);
    }
});

app.get("/api/books", async (req, res) => {
  res.json(await searchBooks(req.query.search)); 
});

app.post("/api/books", LoggedInOnly, async (req, res) => {
  const { author, title, details, categoryIds } = req.body;
  const result = await newBook(author, title, details, categoryIds);
  if (typeof result == "string") {
    APIError(res, result);
  } else res.status(201).json(result);
});

app.put("/api/books/:id", LoggedInOnly, async (req, res) => {
  const id = req.params.id;
  if (isNaN(parseInt(id))) return APIError(res, "Invalid book ID.");
  const { author, title, details, categoryIds } = req.body;
  const result = await editBook(parseInt(id), author, title, details, categoryIds);
  if (typeof result == "string") {
    APIError(res, result);
  } else res.json(result);
});

app.delete("/api/books/:id", LoggedInOnly, async (req, res) => {
  const id = req.params.id;
  if (isNaN(parseInt(id))) return APIError(res, "Invalid book ID.");
  const book = await Book.findByPk(parseInt(id));
  if (book) {
    await book.destroy();
    res.status(204).end();
  } else APIError(res, "No book with that id.", 404);
});

app.get("/api/books/search/:title", async (req, res) => {
  const search = req.params.title;
  res.json(await searchBooks(search)); 
});

app.get("/api/books/:id", async (req, res) => {
  const id = req.params.id;
  if (isNaN(parseInt(id))) return APIError(res, "Invalid book ID.");
  const book = await Book.findByPk(parseInt(id), {
    include: [{ model: Category, as: 'categories', through: { attributes: [] } }]
  });
  if (book) {
    res.json(book);
  } else {
    APIError(res, "Book not found", 404);
  }
});

app.get("/api/books/category/:categoryId", async (req, res) => {
  const { categoryId } = req.params;
  if (isNaN(parseInt(categoryId))) return APIError(res, "Invalid category ID.");
  const books = await getBooksByCategory(parseInt(categoryId));
  res.json(books); 
});

app.get("/api/categories/stats", async (req, res) => {
  const stats = await getCategoryStats();
  res.json(stats);
});

app.put("/api/categories/:id", LoggedInOnly, async (req, res) => {
  const { id } = req.params;
  if (isNaN(parseInt(id))) return APIError(res, "Invalid category ID.");
  const { name } = req.body;
  if (!name) return APIError(res, "Category name is required for update.");
  const result = await editCategoryFunc(parseInt(id), name);
  if (typeof result === "string") {
    APIError(res, result);
  } else {
    res.json(result);
  }
});

app.delete("/api/categories/:id", LoggedInOnly, async (req, res) => {
  const { id } = req.params;
  if (isNaN(parseInt(id))) return APIError(res, "Invalid category ID.");
  const result = await deleteCategoryFunc(parseInt(id));
  if (typeof result === "string") { 
    APIError(res, result, result === "Category not found." ? 404 : 400);
  } else if (result === true) { 
    res.status(204).end();
  } else { 
    APIError(res, "Failed to delete category for an unknown reason.", 500);
  }
});