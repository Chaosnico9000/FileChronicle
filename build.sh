#!/bin/bash
# Build script for FileChronicle (Linux/macOS)

set -e

CONFIGURATION="Release"
CLEAN=false
PUBLISH=false
PACK=false
RUNTIME=""

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --configuration|-c)
            CONFIGURATION="$2"
            shift 2
            ;;
        --clean)
            CLEAN=true
            shift
            ;;
        --publish)
            PUBLISH=true
            shift
            ;;
        --pack)
            PACK=true
            shift
            ;;
        --runtime|-r)
            RUNTIME="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║  FileChronicle Build Script                                  ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""

PROJECT_FILE="FileChronicle.csproj"
PUBLISH_DIR="publish"

# Clean
if [ "$CLEAN" = true ]; then
    echo "🧹 Cleaning previous builds..."
    rm -rf bin obj "$PUBLISH_DIR"
    echo "✅ Clean complete"
    echo ""
fi

# Restore
echo "📦 Restoring packages..."
dotnet restore "$PROJECT_FILE"
echo "✅ Restore complete"
echo ""

# Build
echo "🔨 Building project ($CONFIGURATION)..."
dotnet build "$PROJECT_FILE" --configuration "$CONFIGURATION" --no-restore
echo "✅ Build complete"
echo ""

# Publish
if [ "$PUBLISH" = true ]; then
    echo "📤 Publishing..."
    
    if [ -n "$RUNTIME" ]; then
        RUNTIMES=("$RUNTIME")
    else
        RUNTIMES=("linux-x64" "linux-arm64" "osx-x64" "osx-arm64")
    fi
    
    for rid in "${RUNTIMES[@]}"; do
        echo "  Publishing for $rid..."
        
        OUTPUT_PATH="$PUBLISH_DIR/$rid"
        
        dotnet publish "$PROJECT_FILE" \
            --configuration "$CONFIGURATION" \
            --runtime "$rid" \
            --self-contained true \
            --output "$OUTPUT_PATH" \
            /p:PublishSingleFile=true \
            /p:IncludeNativeLibrariesForSelfExtract=true
        
        echo "  ✅ Published to $OUTPUT_PATH"
    done
    
    echo "✅ Publish complete"
    echo ""
fi

# Pack
if [ "$PACK" = true ]; then
    echo "📦 Creating packages..."
    
    PACKAGES_DIR="$PUBLISH_DIR/packages"
    mkdir -p "$PACKAGES_DIR"
    
    for dir in "$PUBLISH_DIR"/*; do
        if [ -d "$dir" ] && [ "$(basename "$dir")" != "packages" ]; then
            RID_NAME=$(basename "$dir")
            ZIP_FILE="$PACKAGES_DIR/FileChronicle-$RID_NAME.tar.gz"
            
            echo "  Creating $ZIP_FILE..."
            
            tar -czf "$ZIP_FILE" -C "$dir" .
            
            echo "  ✅ Created $ZIP_FILE"
        fi
    done
    
    echo "✅ Pack complete"
    echo ""
fi

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║  Build Complete!                                             ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""

if [ "$PUBLISH" = true ]; then
    echo "Published artifacts are in: $PUBLISH_DIR"
fi

echo "You can run the application with:"
echo "  dotnet run -- snapshot /tmp/test output.json"
echo ""
