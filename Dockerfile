# to run: docker build . -t g2 && docker run --rm -it     -e TKEY=123 -e YKEY=321 g2
FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY TelegramClient/*.csproj ./
RUN dotnet restore

COPY TelegramClient/. ./
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /appr
COPY --from=build-env /app/out .
RUN echo $CI_pub_test $CI_prv_test
ENTRYPOINT  echo $CI_pub_test $CI_prv_test qqq $Test1 $Test2 \
&& sed -i 's/_telegramKey/'$_ttsToWhom'/g' cfg.json \
&& sed -i 's/_ttsToWhom/'$_ttsToWhom'/g' cfg.json \
&& sed -i 's/_phone/'$_phone'/g' cfg.json \
&& sed -i 's/_logBack/'$_logBack'/g' cfg.json \
&& sed -i 's/_YaTtsKey/'$_YaTtsKey'/g' cfg.json \
bash cat cfg.json
#&& dotnet  TelegramClient.dll
