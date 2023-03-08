Unity VOICEVOX Bridge
===============

Unity上でVOICEVOX EngineのAPIを利用し、簡単に合成音声の再生ができるシンプルなライブラリです。

## VOICEVOXについて
詳細な情報や構成、生成した音声に関する利用規約については、以下の公式サイト、Githubリポジトリを参照してください。

- [VOICEVOX](https://voicevox.hiroshiba.jp/)
- [VOICEVOX Engine](https://github.com/VOICEVOX/voicevox_engine)

## Package Manager
UPMを利用してインストールする場合は、PacakgeMangerからgit URLとして`https://github.com/mikito/unity-voicevox-bridge.git?path=Assets/VoicevoxBridge/`を追加してください。

もしくは、Packages/manifest.jsonに以下を追記してください。

```json
{
  "dependencies": {
    "net.mikinya.unity-voicevox-bridge": "https://github.com/mikito/unity-voicevox-bridge.git?path=Assets/VoicevoxBridge/",
    ...
  }
}
```

## VOICEVOX Engineの起動 

このライブラリを利用するためには、VOICEVOX EngineのサーバーをUnityとは別に起動しておく必要があります。いくつか方法を示します。

### A. VOICEVOXエディターを利用する場合（簡単）

以下の公式サイトからVOICEVOXのアプリケーション（VOICEVOXエディター）をダウンロードし起動します。

https://voicevox.hiroshiba.jp/

エディターにはVOICEVOX Engineが内包されており、別サービスからリクエストを受け付けることができます。

### B. Dockerを利用する場合 

Dockerを利用する場合は以下のコマンドでVOICEVOX EngineのDocker Imageを利用できます。

https://github.com/VOICEVOX/voicevox_engine#docker-%E3%82%A4%E3%83%A1%E3%83%BC%E3%82%B8

また、以下のcompose.ymlファイルを作成し、docker composeコマンド利用するもの簡単です。
```yml
services:
  voicevox_engine_cpu:
    profiles: [cpu]
    image: voicevox/voicevox_engine:cpu-ubuntu20.04-latest
    ports:
      - "50021:50021"
    tty: true
  voicevox_engine_gpu:
    profiles: [gpu]
    image: voicevox/voicevox_engine:nvidia-ubuntu20.04-latest
    ports:
      - "50021:50021"
    tty: true
```

```sh
$ docker compose --profile cpa up # CPU版の起動
$ docker compose --profile gpu up # GPU版の起動
```

### C. ビルド済みVOICEVOX Engineを単独で利用する場合

[こちら](https://github.com/VOICEVOX/voicevox_engine/tags)から対応するビルドアーカイブをダウンロードし梱包されているrunバイナリを実行します。

なお、PCのローカル以外でVOICEVOX Engineのサーバーを立てアクセスする場合は、以下のようにhostオプションが必要です。
```sh
$ ./run --host 192.168.xxx.xxx # サーバーPCのIPアドレス 
```

## 使い方

上記に従ってVOICEVOICE Engineを起動し、Unity上の任意のGameObjectにVOICEVOXコンポーネントをアタッチしてください。

<img width="339" alt="スクリーンショット 2023-03-08 21 12 08" src="https://user-images.githubusercontent.com/1071168/223710290-fcce42ff-9937-4fcb-acbe-b3baf66aef7f.png">

もし、VOICEVOX Engineをデフォルト以外で起動している場合はVoicevox Server URLを変更してください（default: http://localhost:50021/ ）。

必要に応じて任意のAudioSourceを設定します。設定しない場合、起動後に同一オブジェクトに自動的にアタッチされます。

### 簡単な音声の合成と再生

以下のように、VoicevoxBridge名前空間をusingし、VOICEVOXコンポーネントのPlayOneShot関数を呼び出します。PlayOneShot関数は音声合成のリクエストから再生までを続けて行います。

```cs
using VoicevoxBridge;

[SerializeField] VOICEVOX voicevox;
...

public async void PlayVoice()
{
    int speaker = 1; // ずんだもん あまあま
    string text = "ずんだもんなのだ";
    await voicevox.PlayOneShot(speaker, text);
}
```

### 音声の合成と再生を分けた呼び出し
音声合成自体の終了タイミングを制御したり、予め複数のテキストを合成し連続再生するような場合は、以下のようにCreateVoice関数とPlay関数の呼び出しに分けることができます。

```cs
using VoicevoxBridge;

[SerializeField] VOICEVOX voicevox;
...

public async void PlayVoice()
{
    int speaker = 1; // ずんだもん あまあま
    string text = "ずんだもんなのだ";
    Voice voice = await voicevox.CreateVoice(speaker, text);
    await voicevox.Play(voice);
}
```

Voiceオブジェクトは内部にAudioClipを保持しています。Play関数に渡すことで再生完了後に自動的にリソースを破棄しますが、Voiceオブジェクトを使い回したい場合などは、autoReleaseVoiceオプションにfalseを渡してください。この場合、Voiceオブジェクトが不要になったタイミングでVoice.Dispose()を呼び出してください。

### 注意事項
このライブラリはあくまでVOICEVOX Engineのサーバーを簡単に利用するためのものです。PC, iOS, Androidアプリなど、ビルドした成果物のみで音声合成を実行することはできません。別途、VOICEVOX Engineサーバーを用意し、アプリケーションから通信する必要があります。

また、現在のところ、クエリ編集、プリセットの利用、辞書登録などの機能は実装されていません。

## License
This library is under the MIT License. 
