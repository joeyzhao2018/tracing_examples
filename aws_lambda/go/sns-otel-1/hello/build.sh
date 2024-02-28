set -e

mkdir -p "asset-output"
echo "Building Go binaries"
# CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -ldflags="-s -w" -o build/bootstrap hello/main.go
GOCACHE=/tmp go mod tidy && GOCACHE=/tmp GOARCH=amd64 GOOS=linux go build -tags lambda.norpc -o ./asset-output/bootstrap main.go


echo "zipping"
rm ./asset-output/hello.zip
zip -j ./asset-output/hello.zip ./asset-output/bootstrap

