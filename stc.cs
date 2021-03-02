###
 * 今天让我们来写一个编译器，一个超级无敌小的编译器！它小到如果把所有注释删去的话，大概只剩
 * 200行左右的代码。
 * 
 * 我们将会用它将 lisp 风格的函数调用转换为 C 风格。
 *
 * 如果你对这两种风格不是很熟悉，下面是一个简单的介绍。
 *
 * 假设我们有两个函数，`add` 和 `subtract`，那么它们的写法将会是下面这样：
 * 
 *                  LISP                      C
 *
 *   2 + 2          (add 2 2)                 add(2, 2)
 *   4 - 2          (subtract 4 2)            subtract(4, 2)
 *   2 + (4 - 2)    (add 2 (subtract 4 2))    add(2, subtract(4, 2))
 *
 * 很简单对吧？
 *
 * 这个转换就是我们将要做的事情。虽然这并不包含 LISP 或者 C 的全部语法，但它足以向我们展示现代编译器很多要点。
 ************************************************************
 * 大多数编译器可以分成三个阶段：解析（Parsing），转换（Transformation）以及代码生成（Code Generation）
 *
 * 1. *解析* 将原始代码转化为易于后续处理的结构化数据(例如json), 处理为AST(可以认为是一个json)
 *
 * 2. *转换* 将处理AST(json)，这一步类似于正则处理字符串.
 *
 * 3. *代码生成* 把改好的AST转成新的代码, 可以认为是第一步的逆操作.
 ************************************************************
 * 解析（Parsing）
 * -------
 *
 * 解析一般来说会分成两个阶段：词法分析（Lexical Analysis）和语法分析（Syntactic Analysis）。
 *
 * 1. *词法分析*接收原始代码，然后把它通通分割成很多Token(可以理解为把句子拆成一个个单词)，这个过程是在词法分析器（Tokenizer或者Lexer）中完成的。
 *    用一个数组存储所有token，他们是代码语句的碎片. 可以是数字、标签、标点符号、运算符，或者其它任何东西。
 *
 * 2. *语法分析* 接收之前生成的 Token数组，把它们转换成易于处理的树状结构。这被称为中间表示（intermediate representation）或抽象语法树（Abstract Syntax Tree， 缩写为AST）
 *    AST是一个嵌套程度很深的json，用一种更容易处理的方式代表了代码本身，因为他不仅结构化, 而且提供了很多冗余信息。
 *
 * 比如说对于下面这一行代码语句：
 *
 *   (add 2 (subtract 4 2)) 
 *   coffee形式: 
 *   ..add 2
 *		 ..subtract 4 2
 *
 * Token数组, 注意上面的所有元素都平铺在数组里面: 
 *
 *   [
 *     { type: 'paren',  value: '('        },
 *     { type: 'name',   value: 'add'      },
 *     { type: 'number', value: '2'        },
 *     { type: 'paren',  value: '('        },
 *     { type: 'name',   value: 'subtract' },
 *     { type: 'number', value: '4'        },
 *     { type: 'number', value: '2'        },
 *     { type: 'paren',  value: ')'        },
 *     { type: 'paren',  value: ')'        }
 *   ]
 * 抽象语法树AST, 这里是一个键值对组成的树:
 *
 *   {
 *     type: 'Program',
 *     body: [{
 *       type: 'CallExpression',
 *       name: 'add',
 *       params: [{
 *         type: 'NumberLiteral',
 *         value: '2'
 *       }, {
 *         type: 'CallExpression',
 *         name: 'subtract',
 *         params: [{
 *           type: 'NumberLiteral',
 *           value: '4'
 *         }, {
 *           type: 'NumberLiteral',
 *           value: '2'
 *         }]
 *       }]
 *     }]
 *   }
 ************************************************************
 * 转换（Transformation）
 * --------------
 *
 * 编译器的下一步就是转换。它只是把 AST 拿过来然后对它做一些修改。它可以在同种语言下操作 AST，也可以把 AST 翻译成全新的语言。c族/java族/js族就是都翻译为基础语言c/java/js.
 *
 * 下面我们来看看该如何转换 AST。
 *
 * 你或许注意到了我们的 AST 中有很多相似的元素，这些元素都有 type 属性，它们被称为 AST结点。这些结点含有若干属性，可以用于描述 AST 的部分信息。
 *
 * 比如下面是一个“NumberLiteral”结点：
 *
 *   {
 *     type: 'NumberLiteral',
 *     value: '2'
 *   }
 *
 * 又比如下面是一个“CallExpression”结点：
 *
 *   {
 *     type: 'CallExpression',
 *     name: 'subtract',
 *     params: [...nested nodes go here...]
 *   }
 *
 * 当转换AST的时候我们可以添加、移动、替代这些结点，也可以根据现有的 AST 生成一个全新的AST
 *
 * 既然我们编译器的目标是把输入的代码转换为一种新的语言，所以我们将会着重于产生一个针对新语言的全新的AST。
 * 
 *
 * 遍历（Traversal）
 * ---------
 *
 * 为了能处理所有的结点，我们需要遍历它们，使用的是深度优先遍历。
 *
 *   {
 *     type: 'Program',
 *     body: [{
 *       type: 'CallExpression',
 *       name: 'add',
 *       params: [{
 *         type: 'NumberLiteral',
 *         value: '2'
 *       }, {
 *         type: 'CallExpression',
 *         name: 'subtract',
 *         params: [{
 *           type: 'NumberLiteral',
 *           value: '4'
 *         }, {
 *           type: 'NumberLiteral',
 *           value: '2'
 *         }]
 *       }]
 *     }]
 *   }
 *
 * 对于上面的 AST 的遍历流程是这样的：
 *
 *   1. Program - 从 AST 的顶部结点开始
 *   2. CallExpression (add) - Program 的第一个子元素
 *   3. NumberLiteral (2) - CallExpression (add) 的第一个子元素
 *   4. CallExpression (subtract) - CallExpression (add) 的第二个子元素
 *   5. NumberLiteral (4) - CallExpression (subtract) 的第一个子元素
 *   6. NumberLiteral (2) - CallExpression (subtract) 的第二个子元素
 *
 * 如果我们直接在 AST 内部操作，而不是产生一个新的 AST，那么就要在这里介绍所有种类的抽象，
 * 但是目前访问（visiting）所有结点的方法已经足够了。
 *
 * 使用“访问（visiting）”这个词的是因为这是一种模式，代表在对象结构内对元素进行操作。
 *
 * 访问者（Visitors）
 * --------
 *
 * 我们最基础的想法是创建一个“访问者（visitor）”对象，这个对象中包含一些方法，可以处理不同的结点。
 *
 *   visitor = {
 *     NumberLiteral() {},
 *     CallExpression() {}
 *   };
 *
 * 当我们遍历 AST 的时候，如果遇到了匹配 type 的结点，我们可以调用 visitor 中的方法。
 *
 * 为了使这个对象能够正常工作，我们需要传入当前节点以及当前节点的父节点的引用。
 *
 *   visitor = {
 *     NumberLiteral(node, parent) {},
 *     CallExpression(node, parent) {},
 *   };
 *
 * 然而，也存在着在“离开”节点的时候调用方法的可能性。假设我们有以下的抽象语法树结构：
 *
 *   - Program
 *     - CallExpression
 *       - NumberLiteral
 *       - CallExpression
 *         - NumberLiteral
 *         - NumberLiteral
 *
 * 当我们向下遍历语法树的时候，我们会碰到所谓的叶子节点。我们在处理完一个节点后会“离开”这个节点。
 * 所以向下遍历树的时候我们“进入”节点，而向上返回的时候我们“离开”节点。
 *
 *   -> Program (enter)
 *     -> CallExpression (enter)
 *       -> Number Literal (enter)
 *       <- Number Literal (exit)
 *       -> Call Expression (enter)
 *          -> Number Literal (enter)
 *          <- Number Literal (exit)
 *          -> Number Literal (enter)
 *          <- Number Literal (exit)
 *       <- CallExpression (exit)
 *     <- CallExpression (exit)
 *   <- Program (exit)
 *
 * 为了支持上面所讲的功能，我们的访问者对象的最终形态如下：
 *
 *   visitor = {
 *     NumberLiteral: {
 *       enter(node, parent) {},
 *       exit(node, parent) {},
 *     }
 *   };
 ********************************************************
 * 代码生成（Code Generation）
 * ---------------
 *
 * 编译器的最后一个阶段是代码生成，这个阶段做的事情有时候会和转换（transformation）重叠，
 * 但是代码生成最主要的部分还是根据 AST 来输出代码。
 *
 * 代码生成有几种不同的工作方式，有些编译器将会重用之前生成的 token，有些会创建独立的代码表示，以便于线性地输出代码。但是接下来我们还是着重于使用之前生成好的 AST。
 *
 * 我们的代码生成器需要知道如何“打印”AST 中所有类型的结点，然后它会递归地调用自身，直到所有代码都被打印到一个很长的字符串中。
 * 
 *********************************************************
 * 好了！这就是编译器中所有的部分了。
 *
 * 当然不是说所有的编译器都像我说的这样。不同的编译器有不同的目的，所以也可能需要不同的步骤。
 *
 * 但你现在应该对编译器到底是个什么东西有个大概的认识了。
 *
 * 既然我全都解释一遍了，你应该能写一个属于自己的编译器了吧？
 *
 * 哈哈开个玩笑，接下来才是重点 :P
 *
 * 所以我们开始吧...
 * ============================================================================
 *                                   (/^▽^)/
 *                            词法分析器（Tokenizer）!
 * ============================================================================

 * 我们从第一个阶段开始，即词法分析，使用的是词法分析器（Tokenizer）。
 *
 * 我们只是接收代码组成的字符串，然后把它们分割成 token 组成的数组。
 *
 *   (add 2 (subtract 4 2))   =>   [{ type: 'paren', value: '(' }, ...]
 * 输入正常lisp语句:   (add 2 (subtract 4 2))
 *
 * 输出: Token数组: 
 *
 *   [
 *     { type: 'paren',  value: '('        },
 *     { type: 'name',   value: 'add'      },
 *     { type: 'number', value: '2'        },
 *     { type: 'paren',  value: '('        },
 *     { type: 'name',   value: 'subtract' },
 *     { type: 'number', value: '4'        },
 *     { type: 'number', value: '2'        },
 *     { type: 'paren',  value: ')'        },
 *     { type: 'paren',  value: ')'        }
 *   ]
 *
###

# 我们从接收一个字符串开始，首先设置两个变量。
tokenizer=(input)->

  # `current`变量类似指针，用于记录我们在代码字符串中的位置。
  current = 0

  # `tokens`数组是我们放置 token 的地方
  tokens = []

  # 首先我们创建一个 `while` 循环， `current` 变量会在循环中自增。
  # 
  # 我们这么做的原因是，由于 token 数组的长度是任意的，所以可能要在单个循环中多次
  # 增加 `current` 
  while current < input.length

    # 我们在这里储存了 `input` 中的当前字符
    char = input[current]

    # 要做的第一件事情就是检查是不是右圆括号。这在之后将会用在 `CallExpressions` 中，
    # 但是现在我们关心的只是字符本身。
    #
    # 检查一下是不是一个左圆括号。
    if char === '('

      # 如果是，那么我们 push 一个 type 为 `paren`，value 为左圆括号的对象。
      tokens.push
        type: 'paren'
        value: '('
      
      # 自增 `current`
      current++

      # 结束本次循环，进入下一次循环
      continue
    

    # 然后我们检查是不是一个右圆括号。这里做的时候和之前一样：检查右圆括号、加入新的 token、
    # 自增 `current`，然后进入下一次循环。
    if char === ')' 
      tokens.push
        type: 'paren'
        value: ')'
      
      current++
      continue
    

    # 继续，我们现在检查是不是空格。有趣的是，我们想要空格的本意是分隔字符，但这现在
    # 对于我们储存 token 来说不那么重要。我们暂且搁置它。
    # 
    # 所以我们只是简单地检查是不是空格，如果是，那么我们直接进入下一个循环。
    WHITESPACE = /\s/
    if WHITESPACE.test char
      current++
      continue
    

    # 下一个 token 的类型是数字。它和之前的 token 不同，因为数字可以由多个数字字符组成，
    # 但是我们只能把它们识别为一个 token。
    # 
    #   (add 123 456)
    #        ^^^ ^^^
    #        Only two separate tokens
    #        这里只有两个 token
    #        
    # 当我们遇到一个数字字符时，将会从这里开始。
    NUMBERS = /[0-9]/
    if NUMBERS.test char

      # 创建一个 `value` 字符串，用于 push 字符。
      value = ''

      # 然后我们循环遍历接下来的字符，直到我们遇到的字符不再是数字字符为止，把遇到的每
      # 一个数字字符 push 进 `value` 中，然后自增 `current`。
      while NUMBERS.test char
        value += char
        char = input[++current]
      

      # 然后我们把类型为 `number` 的 token 放入 `tokens` 数组中。
      tokens.push
        type: 'number'
        value: value
      

      # 进入下一次循环。
      continue

    # 我们同样支持字符串，字符串是由双引号"包裹的文字内容。
    #
    #   (concat "foo" "bar")
    #            ^^^   ^^^ string tokens
    #
    # 我们引号开始检测。
    if char == '"'
      # 创造一个`value`变量保存我们的字符串。
      value = ''

      # 跳过意味着字符串开始的那个引号。
      char = input[++current]

      # 之后我们遍历每一个字符直到我们到达了另一个引号。
      while char != '"'
        value += char
        char = input[++current]
      
      # 跳过意味着字符串结尾的引号。
      char = input[++current]
			# 然后创造字符串词素并添加到`tokens`数组
      okens.push
        type: 'string'
        value # 这个会有问题, 如果引入单一对象参数就没问题了.

      continue
    
    

    # 最后一种类型的 token 是 `name`。它由一系列的字母组成，这在我们的 lisp 语法中
    # 代表了函数。
    #
    #   (add 2 4)
    #    ^^^
    #    Name token
    #
    LETTERS = /[a-z]/i
    if LETTERS.test char
      value = '';

      # 同样，我们用一个循环遍历所有的字母，把它们存入 value 中。
      while LETTERS.test char
        value += char;
        char = input[++current]
      

      # 然后添加一个类型为 `name` 的 token，然后进入下一次循环。
      tokens.push
        type: 'name',
        value: value
      

      continue
    

    # 最后如果我们没有匹配上任何类型的 token，那么我们抛出一个错误。
    throw new TypeError 'I dont know what this character is: ' + char
  

  # 词法分析器的最后我们返回 tokens 数组。
  tokens


###
 * ============================================================================
 *                                 ヽ/❀o ل͜ o\ﾉ
 *                             语法分析器（Parser）!!!
 * ============================================================================
###

###
 *  语法分析器接受 token 数组，然后把它转化为 AST
 *
 *   [{ type: 'paren', value: '(' }, ...]   =>   { type: 'Program', body: [...] }
 * 输入: Token数组, 注意上面的所有元素都平铺在数组里面: 
 *
 *   [
 *     { type: 'paren',  value: '('        },
 *     { type: 'name',   value: 'add'      },
 *     { type: 'number', value: '2'        },
 *     { type: 'paren',  value: '('        },
 *     { type: 'name',   value: 'subtract' },
 *     { type: 'number', value: '4'        },
 *     { type: 'number', value: '2'        },
 *     { type: 'paren',  value: ')'        },
 *     { type: 'paren',  value: ')'        }
 *   ]
 *
 * 输出抽象语法树AST, 这里是一个键值对组成的树:
 *
 *   {
 *     type: 'Program',
 *     body: [{
 *       type: 'CallExpression',
 *       name: 'add',
 *       params: [{
 *         type: 'NumberLiteral',
 *         value: '2'
 *       }, {
 *         type: 'CallExpression',
 *         name: 'subtract',
 *         params: [{
 *           type: 'NumberLiteral',
 *           value: '4'
 *         }, {
 *           type: 'NumberLiteral',
 *           value: '2'
 *         }]
 *       }]
 *     }]
 *   }
###

# 现在我们定义 parser 函数，接受 `tokens` 数组
parser=(tokens) ->

  # 我们再次声明一个 `current` 变量作为指针。
  current = 0;

  # 但是这次我们使用递归而不是 `while` 循环，所以我们定义一个 `walk` 函数。
  walk=() ->

    # walk函数里，我们从当前token开始
    token = tokens[current];

    # 对于不同类型的结点，对应的处理方法也不同，我们从 `number` 类型的 token 开始。
    # 检查是不是 `number` 类型
    if (token.type === 'number') {

      # 如果是，`current` 自增。
      current++;

      # 然后我们会返回一个新的 AST 结点 `NumberLiteral`，并且把它的值设为 token 的值。
      return {
        type: 'NumberLiteral',
        value: token.value
      };
    }

    # 接下来我们检查是不是 CallExpressions 类型，我们从左圆括号开始。
    if (
      token.type === 'paren' &&
      token.value === '('
    ) {

      # 我们会自增 `current` 来跳过这个括号，因为括号在 AST 中是不重要的。
      token = tokens[++current];

      # 我们创建一个类型为 `CallExpression` 的根节点，然后把它的 name 属性设置为当前
      # token 的值，因为紧跟在左圆括号后面的 token 一定是调用的函数的名字。 
      node = {
        type: 'CallExpression',
        name: token.value,
        params: []
      };

      # 我们再次自增 `current` 变量，跳过当前的 token 
      token = tokens[++current];

      # 现在我们循环遍历接下来的每一个 token，直到我们遇到右圆括号，这些 token 将会
      # 是 `CallExpression` 的 `params`（参数）
      # 
      # 这也是递归开始的地方，我们采用递归的方式来解决问题，而不是去尝试解析一个可能有无限
      # 层嵌套的结点。
      # 
      # 为了更好地解释，我们来看看我们的 Lisp 代码。你会注意到 `add` 函数的参数有两个，
      # 一个是数字，另一个是一个嵌套的 `CallExpression`，这个 `CallExpression` 中
      # 包含了它自己的参数（两个数字）
      #
      #   (add 2 (subtract 4 2))
      # 
      # 你也会注意到我们的 token 数组中有多个右圆括号。
      #
      #   [
      #     { type: 'paren',  value: '('        },
      #     { type: 'name',   value: 'add'      },
      #     { type: 'number', value: '2'        },
      #     { type: 'paren',  value: '('        },
      #     { type: 'name',   value: 'subtract' },
      #     { type: 'number', value: '4'        },
      #     { type: 'number', value: '2'        },
      #     { type: 'paren',  value: ')'        }, <<< 右圆括号
      #     { type: 'paren',  value: ')'        }  <<< 右圆括号
      #   ]
      #
      # 遇到嵌套的 `CallExpressions` 时，我们将会依赖嵌套的 `walk` 函数来
      # 增加 `current` 变量
      # 
      # 所以我们创建一个 `while` 循环，直到遇到类型为 `'paren'`，值为右圆括号的 token。 
      while (
        (token.type !== 'paren') ||
        (token.type === 'paren' && token.value !== ')')
      ) {
        # 我们调用 `walk` 函数，它将会返回一个结点，然后我们把这个节点
        # 放入 `node.params` 中。
        node.params.push(walk());
        token = tokens[current];
      }

      # 我们最后一次增加 `current`，跳过右圆括号。
      current++;

      # 返回结点。
      return node;
    }

    # 同样，如果我们遇到了一个类型未知的结点，就抛出一个错误。
    throw new TypeError(token.type);
  

  # 现在，我们创建 AST，根结点是一个类型为 `Program` 的结点。
  ast = {
    type: 'Program',
    body: []
  };

  # 现在我们开始 `walk` 函数，把结点放入 `ast.body` 中。
  #
  # 之所以在一个循环中处理，是因为我们的程序可能在 `CallExpressions` 后面包含连续的两个
  # 参数，而不是嵌套的。
  #
  #   (add 2 2)
  #   (subtract 4 2)
  #
  while (current < tokens.length) {
    ast.body.push(walk());
  }

  # 最后我们的语法分析器返回 AST 
  return ast;


###
 * ============================================================================
 *                                 ⌒(❀>◞౪◟<❀)⌒
 *                                   遍历器!!!
 * ============================================================================
###

###
 * 现在我们有了 AST，我们需要一个 visitor 去遍历所有的结点。当遇到某个类型的结点时，我们
 * 需要调用 visitor 中对应类型的处理函数。
 *
 *   traverse(ast, {
 *     Program(node, parent) {
 *       # ...
 *     },
 *
 *     CallExpression(node, parent) {
 *       # ...
 *     },
 *
 *     NumberLiteral(node, parent) {
 *       # ...
 *     }
 *   });
###

# 所以我们定义一个遍历器，它有两个参数，AST 和 vistor。在它的里面我们又定义了两个函数...
traverser=(ast, visitor) ->

  # `traverseArray` 函数允许我们对数组中的每一个元素调用 `traverseNode` 函数。
  traverseArray=(array, parent)-> {
    array.forEach((child)->{
      traverseNode(child, parent);
    });
  }

  # `traverseNode` 函数接受一个 `node` 和它的父结点 `parent` 作为参数，这个结点会被
  # 传入到 visitor 中相应的处理函数那里。
  traverseNode=(node, parent)-> 

    # 首先我们看看 visitor 中有没有对应 `type` 的处理函数。
    method = visitor[node.type];

    # 如果有，那么我们把 `node` 和 `parent` 都传入其中。
    if method
      method(node, parent)
    

    # 下面我们对每一个不同类型的结点分开处理。
    switch node.type

      # 我们从顶层的 `Program` 开始，Program 结点中有一个 body 属性，它是一个由若干
      # 个结点组成的数组，所以我们对这个数组调用 `traverseArray`。
      #
      # （记住 `traverseArray` 会调用 `traverseNode`，所以我们会递归地遍历这棵树。）
      when 'Program'
        traverseArray(node.body, node);


      # 下面我们对 `CallExpressions` 做同样的事情，遍历它的 `params`。
      when 'CallExpression'
        traverseArray(node.params, node);


      # 如果是 `NumberLiterals`，那么就没有任何子结点了，所以我们直接 break
      when 'NumberLiteral'


      # 同样，如果我们不能识别当前的结点，那么就抛出一个错误。
      else
        throw new TypeError(node.type);
    
  

  # 最后我们对 AST 调用 `traverseNode`，开始遍历。注意 AST 并没有父结点。
  traverseNode(ast, null)


###
 * ============================================================================
 *                                   ⁽(◍˃̵͈̑ᴗ˂̵͈̑)⁽
 *                                   转换器!!!
 * ============================================================================
###

###
 * 下面是转换器。转换器接收我们在之前构建好的 AST，然后把它和 visitor 传递进入我们的遍历
 * 器中 ，最后得到一个新的 AST。
 *
 * ----------------------------------------------------------------------------
 *            原始的 AST               |               转换后的 AST
 * ----------------------------------------------------------------------------
 *   {                                |   {
 *     type: 'Program',               |     type: 'Program',
 *     body: [{                       |     body: [{
 *       type: 'CallExpression',      |       type: 'ExpressionStatement',
 *       name: 'add',                 |       expression: {
 *       params: [{                   |         type: 'CallExpression',
 *         type: 'NumberLiteral',     |         callee: {
 *         value: '2'                 |           type: 'Identifier',
 *       }, {                         |           name: 'add'
 *         type: 'CallExpression',    |         },
 *         name: 'subtract',          |         arguments: [{
 *         params: [{                 |           type: 'NumberLiteral',
 *           type: 'NumberLiteral',   |           value: '2'
 *           value: '4'               |         }, {
 *         }, {                       |           type: 'CallExpression',
 *           type: 'NumberLiteral',   |           callee: {
 *           value: '2'               |             type: 'Identifier',
 *         }]                         |             name: 'subtract'
 *       }]                           |           },
 *     }]                             |           arguments: [{
 *   }                                |             type: 'NumberLiteral',
 *                                    |             value: '4'
 * ---------------------------------- |           }, {
 *                                    |             type: 'NumberLiteral',
 *                                    |             value: '2'
 *                                    |           }]
 *         (那一边比较长/w\)           |         }]
 *                                    |       }
 *                                    |     }]
 *                                    |   }
 * ----------------------------------------------------------------------------
###

# 定义我们的转换器函数，接收 AST 作为参数
function transformer(ast) {

  # 创建 `newAST`，它与我们之前的 AST 类似，有一个类型为 Program 的根节点。
  newAst = {
    type: 'Program',
    body: []
  };

  # 下面的代码会有些奇技淫巧，我们在父结点上使用一个属性 `context`（上下文），这样我们就
  # 可以把结点放入他们父结点的 context 中。当然可能会有更好的做法，但是为了简单我们姑且
  # 这么做吧。
  #
  # 注意 context 是一个*引用*，从旧的 AST 到新的 AST。
  ast._context = newAst.body;

  # 我们把 AST 和 visitor 函数传入遍历器
  traverser(ast, {

    # 第一个 visitor 方法接收 `NumberLiterals`。
    NumberLiteral: function(node, parent) {

      # 我们创建一个新结点，名字叫 `NumberLiteral`，并把它放入父结点的 context 中。
      parent._context.push({
        type: 'NumberLiteral',
        value: node.value
      });
    },

    # 下一个，`CallExpressions`。
    CallExpression: function(node, parent) {

      # 我们创建一个 `CallExpression` 结点，里面有一个嵌套的 `Identifier`。
      expression = {
        type: 'CallExpression',
        callee: {
          type: 'Identifier',
          name: node.name
        },
        arguments: []
      };

      # 下面我们在原来的 `CallExpression` 结点上定义一个新的 context，它是 expression
      # 中 arguments 这个数组的引用，我们可以向其中放入参数。
      node._context = expression.arguments;

      # 然后来看看父结点是不是一个 `CallExpression`，如果不是...
      if (parent.type !== 'CallExpression') {

        # 我们把 `CallExpression` 结点包在一个 `ExpressionStatement` 中，这么做是因为
        # 单独存在（原文为top level）的 `CallExpressions` 在 JavaScript 中也可以被当做
        # 是声明语句。
        # 
        # 译者注：比如 `a = foo()` 与 `foo()`，后者既可以当作表达式给某个变量赋值，也
        # 可以作为一个独立的语句存在。
        expression = {
          type: 'ExpressionStatement',
          expression: expression
        };
      }

      # 最后我们把 `CallExpression`（可能是被包起来的） 放入父结点的 context 中。
      parent._context.push(expression);
    }
  });

  # 最后返回创建好的新 AST。
  return newAst;
}

###
 * ============================================================================
 *                               ヾ（〃＾∇＾）ﾉ♪
 *                                代码生成器!!!!
 * ============================================================================
###

###
 * 现在只剩最后一步啦：代码生成器。
 *
 * 我们的代码生成器会递归地调用它自己，把 AST 中的每个结点打印到一个很大的字符串中。
###

function codeGenerator(node) {

  # 对于不同 `type` 的结点分开处理。
  switch (node.type) {

    # 如果是 `Program` 结点，那么我们会遍历它的 `body` 属性中的每一个结点，并且递归地
    # 对这些结点再次调用 codeGenerator，再把结果打印进入新的一行中。
    case 'Program':
      return node.body.map(codeGenerator)
        .join('\n');

    # 对于 `ExpressionStatements`,我们对它的 expression 属性递归调用，同时加入一个
    # 分号。
    case 'ExpressionStatement':
      return (
        codeGenerator(node.expression) +
        ';' # << (...因为我们喜欢用*正确*的方式写代码)
      );

    # 对于 `CallExpressions`，我们会打印出 `callee`，接着是一个左圆括号，然后对
    # arguments 递归调用 codeGenerator，并且在它们之间加一个逗号，最后加上右圆括号。
    case 'CallExpression':
      return (
        codeGenerator(node.callee) +
        '(' +
        node.arguments.map(codeGenerator)
          .join(', ') +
        ')'
      );

    # 对于 `Identifiers` 我们只是返回 `node` 的 name。
    case 'Identifier':
      return node.name;

    # 对于 `NumberLiterals` 我们只是返回 `node` 的 value
    case 'NumberLiteral':
      return node.value;

    # 如果我们不能识别这个结点，那么抛出一个错误。
    default:
      throw new TypeError(node.type);
  }
}

###
 * ============================================================================
 *                                  (۶* ‘ヮ’)۶”
 *                         !!!!!!!!!!!!编译器!!!!!!!!!!!
 * ============================================================================
###

###
 * 最后！我们创建 `compiler` 函数，它只是把上面说到的那些函数连接到一起。
 *
 *   1. input  => tokenizer   => tokens
 *   2. tokens => parser      => ast
 *   3. ast    => transformer => newAst
 *   4. newAst => generator   => output
###

function compiler(input) {
  tokens = tokenizer(input);
  ast    = parser(tokens);
  newAst = transformer(ast);
  output = codeGenerator(newAst);

  # 然后返回输出!
  return output;
}

###
 * ============================================================================
 *                                   (๑˃̵ᴗ˂̵)و
 * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!你做到了!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
 * ============================================================================
###

# 现在导出所有接口...
module.exports = {
  tokenizer: tokenizer,
  parser: parser,
  transformer: transformer,
  codeGenerator: codeGenerator,
  compiler: compiler
};
