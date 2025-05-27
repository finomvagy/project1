import { Sequelize, DataTypes } from "sequelize";

const sequelize = new Sequelize("sqlite:data/db.sqlite");

export const User = sequelize.define("User", {
  name: {
    type: DataTypes.STRING,
    allowNull: false,
    unique: true,
  },
  password: {
    type: DataTypes.STRING,
    allowNull: false,
  },
});

export const Category = sequelize.define("Category", {
  name: {
    type: DataTypes.STRING,
    allowNull: false,
    unique: true,
  },
});

export const Book = sequelize.define("Book", {
  author: {
    type: DataTypes.STRING,
    allowNull: false,
  },
  title: {
    type: DataTypes.STRING,
    allowNull: false,
  },
  details: {
    type: DataTypes.STRING,
    allowNull: false,
  },
});

Book.belongsToMany(Category, {
  through: "BookCategory",
  as: "categories",
  foreignKey: "bookId",
  otherKey: "categoryId",
});
Category.belongsToMany(Book, {
  through: "BookCategory",
  as: "books",
  foreignKey: "categoryId",
  otherKey: "bookId",
});

await sequelize.sync();