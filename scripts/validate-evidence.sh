#!/usr/bin/env bash
set -euo pipefail

MODE="${1:-}"
BASE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
EVIDENCIAS_DIR="$BASE_DIR/docs/evidencias"

if [ "$MODE" != "--all" ]; then
  echo "Uso: $0 --all"
  exit 1
fi

echo "Iniciando validación de 100 carpetas de evidencias (SF-001 a SF-100)..."

ERRORES=0

for i in $(seq 1 100); do
  FOLDER_NAME=$(printf "SF-%03d" "$i")
  FOLDER_PATH="$EVIDENCIAS_DIR/$FOLDER_NAME"

  if [ ! -d "$FOLDER_PATH" ]; then
    echo "ERROR: Falta carpeta $FOLDER_NAME"
    ERRORES=$((ERRORES + 1))
    continue
  fi

  COUNT_MD=$(find "$FOLDER_PATH" -maxdepth 1 -name "*.md" | wc -l)
  if [ "$COUNT_MD" -eq 0 ]; then
    echo "ERROR: Carpeta $FOLDER_NAME no contiene archivos markdown de evidencia"
    ERRORES=$((ERRORES + 1))
  fi
done

P1="/Users/"
P2="C:\\\\"
P3="C:/Users/"
if grep -rnE -i "($P1|$P2|$P3)" "$EVIDENCIAS_DIR/" 2>/dev/null; then
  echo "ERROR: Se detectaron rutas de máquina host en docs/evidencias"
  ERRORES=$((ERRORES + 1))
fi

if [ "$ERRORES" -gt 0 ]; then
  echo "Validación fallida: $ERRORES errores encontrados."
  exit 1
fi

echo "Validación exitosa: Las 100 carpetas de evidencia (SF-001 a SF-100) están completas, documentadas y portables."
exit 0
