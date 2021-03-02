[![The Super Tiny Compiler](https://cloud.githubusercontent.com/assets/952783/21579290/5755288a-cf75-11e6-90e0-029529a44a38.png)](stc.cs)

***Welcome to The Super Tiny Compiler!***

这是一个超级简单的编译器的例子，包含了现代编译器的几个主要部分，用简单易读的 coffee 编写。

- 读源码将会有助于你了解*大多数*编译器从前端到后端是如何工作的。[stc.cs](stc.cs)
- [You can also check it out on Glitch](https://the-super-tiny-compiler.glitch.me/)
- 或者... [看看演讲](https://www.youtube.com/watch?v=Tar4WgAfMr4)

### 为啥我要关心这个？

确实，大多数人在日常工作中没有必要了解编译器都是如何工作的。但是，编译器无处不在，你使用的很多工具的底层原理都是从编译器那儿来的, 例如codemirror/prosemirror, coffeescript, vscode以及他的插件, 各种markdown的阅读器例如typora/bear, webpack, react/RN, 在可预见的未来, 我们依旧会被js族/java族语言, vscode/chrome类编辑器, webpack/node组工具统治, 这些东西基本都会用编译器相关知识, 至少要用ast(抽象语法树), 其实简单地说ast就是一个键值对(java)对象(js)字典(swift/oc), 是最简单的树状数据结构, 甚至于认为就是个json也没毛病, 所以如果你了解这些, 写个编辑器/浏览器插件就不是那么难了.

### 但是编译器太高大上了！

额，确实。但这是各位大神(Haverbeke, joe......）的锅，把一些本应该很通俗易懂的事情弄得太可怕了，让很多人都认为编译器这种东西是可望而不可及的，甚至只有最呆的书呆子才能理解。其实替各位大神也说一句, 很多书籍的作者自身并不是这个领域的大牛, 或者持有非常古老的观念 

### 好吧，所以我该从哪儿开始？

太棒了！直接去看 [stc.cs](stc.cs) 这个文件吧！

### 六分钟之后, 我回来了

好吧, 我的锅, 有哪里是不清楚的, 让我知道, 我改:) 欢迎提PR:)

### 测试

直接运行 `node test.cs`


### coffee的单独目标

1. 验证lornpug思路, 去配对符号是否可行
2. 考虑是否能继续简化这个编译器, 比如visitor模式似乎并不是必须的.
3. 按照接口验证的思路来进行接口描述, 详细描述每个接口的输入和输出.


### 本项目是从以下项目发展(fork)而来
- https://github.com/jamiebuilds/the-super-tiny-compiler
- https://github.com/starkwang/the-super-tiny-compiler-cn
- https://github.com/YongzeYao/the-super-tiny-compiler-CN



[![cc-by-4.0](https://licensebuttons.net/l/by/4.0/80x15.png)](http://creativecommons.org/licenses/by/4.0/)


