# lisp源码:
  (add 2 (subtract 4 2)) 
# coffee形式: 
  ..add 2
		..subtract 4 2
 
# Token数组, 注意上面的所有元素都平铺在数组里面: 

  [
    { type: 'paren',  value: '('        },
    { type: 'name',   value: 'add'      },
    { type: 'number', value: '2'        },
    { type: 'paren',  value: '('        },
    { type: 'name',   value: 'subtract' },
    { type: 'number', value: '4'        },
    { type: 'number', value: '2'        },
    { type: 'paren',  value: ')'        },
    { type: 'paren',  value: ')'        }
  ]
# token的coffee形式:
  ,,
    :: type: 'paren',  value: '('        
    :: type: 'name',   value: 'add'      
    :: type: 'number', value: '2'        
    :: type: 'paren',  value: '('        
    :: type: 'name',   value: 'subtract' 
    :: type: 'number', value: '4'        
    :: type: 'number', value: '2'        
    :: type: 'paren',  value: ')'        
    :: type: 'paren',  value: ')'        
  
# 抽象语法树AST, 这里是一个键值对组成的树:
  {
    type: 'Program',
    body: [{
      type: 'CallExpression',
      name: 'add',
      params: [{
        type: 'NumberLiteral',
        value: '2'
      }, {
        type: 'CallExpression',
        name: 'subtract',
        params: [{
          type: 'NumberLiteral',
          value: '4'
        }, {
          type: 'NumberLiteral',
          value: '2'
        }]
      }]
    }]
  }
# ast的coffee形式:
  ::
    type: 'Program',
    body: ,,::
      type: 'CallExpression'
      name: 'add'
      params: ,,
				:: type: 'NumberLiteral',	value: '2'
				::
					type: 'CallExpression'
					name: 'subtract'
					params: ,,
						::type: 'NumberLiteral',value: '4'
						::type: 'NumberLiteral',value: '2'
# 目标ast
   {
     type: 'Program',
     body: [{
       type: 'ExpressionStatement',
       expression: {
         type: 'CallExpression',
         callee: {
           type: 'Identifier',
           name: 'add'
         },
         arguments: [{
           type: 'NumberLiteral',
           value: '2'
         }, {
           type: 'CallExpression',
           callee: {
             type: 'Identifier',
             name: 'subtract'
           },
           arguments: [{
             type: 'NumberLiteral',
             value: '4'
           }, {
             type: 'NumberLiteral',
             value: '2'
           }]
         } # 这里缺一个中括号, 但是, 很难发现
       }
     }]
   }
# 目标ast的coffee形式
   ::
     type: 'Program'
     body: ,,::
       type: 'ExpressionStatement'
       expression: ::
         type: 'CallExpression'
         callee:: type: 'Identifier', name: 'add'   
         arguments:,,
				 	::type: 'NumberLiteral',value: '2'
					::
           type: 'CallExpression',
           callee:: type: 'Identifier',name: 'subtract'
           arguments:,,
					 	::type: 'NumberLiteral', value: '4'
						::type: 'NumberLiteral', value: '2'
       
       
# 目标js代码
add(2, subtract(4, 2))
# 目标coffee代码
add..2
	subtract..4, 2


# 其实不支持多个参数, 只支持单对象(键值对)参数
add({a:2, b:subtract({a:4, b:2})})
# 因为coffee里面空格也是贪婪的, 所以, 下面两个都可以
add a:2, b:subtract
	a:4,b:2
add a:2, b:subtract a:4,b:2


add({
	a:2, 
	b:subtract({a:4, b:2}),
	c:1
})

add 
	a:2
	b:subtract a:4,b:2
	c:1

# 不行的两个写法
add a:2, b:subtract a:4,b:2,, c:1
add a:2,( b:subtract a:4,b:2), c:1

