import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      globals: globals.browser,
      },
      rules: {
          '@typescript-eslint/no-explicit-any': 'error', // запрет на any
          '@typescript-eslint/ban-ts-comment': 'error', // запрет на комментарии вроде @ts-ignore
          '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }], // переменная или импорт объявлены, но не используются
          'no-console': ['warn', { allow: ['warn', 'error'] }], // запрет на console.log
          'eqeqeq': ['error', 'always'], // заставляет везде использовать строгое равенство
          'react-hooks/exhaustive-deps': 'error', // повышаем warn до error
          'react-refresh/only-export-components': ['warn', { allowConstantExport: true }]
      },
  },
])
